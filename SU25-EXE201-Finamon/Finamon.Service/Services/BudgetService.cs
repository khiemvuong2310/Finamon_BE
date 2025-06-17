using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finamon.Service.Interfaces;

namespace Finamon.Service.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BudgetService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<decimal> CalculateCurrentAmount(int userId, DateTime startDate, decimal? limit)
        {
            if (!limit.HasValue)
                return 0;

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.Date >= startDate &&
                           e.Date <= (startDate.AddMonths(1).AddDays(-1)) &&
                           !e.IsDelete)
                .SumAsync(e => e.Amount);

            return limit.Value - totalExpenses;
        }

        private bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return true;

            return startDate.Value <= endDate.Value;
        }

        public async Task<PaginatedResponse<BudgetResponse>> GetAllBudgetsAsync(BudgetQueryRequest queryRequest)
        {
            var query = _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts.Where(a => !a.IsDelete))
                    .ThenInclude(a => a.Budget)
                .Include(b => b.User)
                .AsQueryable();

            // Default to not showing deleted if not specified in the query
            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(b => b.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(b => !b.IsDelete);
            }

            // Apply filters
            if (queryRequest.MinAmount.HasValue)
            {
                query = query.Where(b => b.Limit >= queryRequest.MinAmount.Value);
            }

            if (queryRequest.MaxAmount.HasValue)
            {
                query = query.Where(b => b.Limit <= queryRequest.MaxAmount.Value);
            }

            if (queryRequest.StartDate.HasValue)
            {
                query = query.Where(b => b.StartDate >= queryRequest.StartDate.Value);
            }

            if (queryRequest.EndDate.HasValue)
            {
                query = query.Where(b => b.EndDate <= queryRequest.EndDate.Value);
            }
            
            if (queryRequest.UserId.HasValue)
            {
                query = query.Where(b => b.UserId == queryRequest.UserId.Value);
            }

            // Apply sorting
            if (queryRequest.SortBy.HasValue)
            {
                query = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.StartDate)
                        : query.OrderBy(b => b.StartDate),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.UpdatedDate)
                        : query.OrderBy(b => b.UpdatedDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.Limit)
                        : query.OrderBy(b => b.Limit),
                    _ => query.OrderByDescending(b => b.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.Id); // Default sort
            }

            var paginatedBudgets = await PaginatedResponse<Budget>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var budgetResponses = _mapper.Map<List<BudgetResponse>>(paginatedBudgets.Items);

            // Calculate CurrentAmount for each budget
            foreach (var budgetResponse in budgetResponses)
            {
                var budget = paginatedBudgets.Items.First(b => b.Id == budgetResponse.Id);
                if (budget.StartDate.HasValue)
                {
                    budgetResponse.CurrentAmount = await CalculateCurrentAmount(budget.UserId, budget.StartDate.Value, budget.Limit);
                }
                else
                {
                    budgetResponse.CurrentAmount = budget.Limit.GetValueOrDefault();
                }

                // Ensure alerts are properly loaded and sorted
                if (budget.Alerts != null)
                {
                    budgetResponse.Alerts = _mapper.Map<ICollection<BudgetAlertResponse>>(
                        budget.Alerts.OrderByDescending(a => a.CreatedAt).ToList()
                    );
                }
            }
            
            return new PaginatedResponse<BudgetResponse>(budgetResponses, paginatedBudgets.TotalCount, paginatedBudgets.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetResponse> GetBudgetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts.Where(a => !a.IsDelete))
                    .ThenInclude(a => a.Budget)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            var budgetResponse = _mapper.Map<BudgetResponse>(budget);
            
            // Calculate CurrentAmount if StartDate is available
            if (budget.StartDate.HasValue)
            {
                budgetResponse.CurrentAmount = await CalculateCurrentAmount(budget.UserId, budget.StartDate.Value, budget.Limit);
            }
            else
            {
                budgetResponse.CurrentAmount = budget.Limit.GetValueOrDefault();
            }

            // Ensure alerts are properly loaded and sorted
            if (budget.Alerts != null)
            {
                budgetResponse.Alerts = _mapper.Map<ICollection<BudgetAlertResponse>>(
                    budget.Alerts.OrderByDescending(a => a.CreatedAt).ToList()
                );
            }

            return budgetResponse;
        }

        public async Task<BudgetResponse> CreateBudgetAsync(BudgetRequestModel request)
        {
            // Validate user exists and is not deleted
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDelete);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found or has been deleted");

            // Validate date range
            if (!IsValidDateRange(request.StartDate, request.EndDate))
                throw new ArgumentException("End date must be greater than or equal to start date");

            // Validate limit
            if (request.Limit <= 0)
                throw new ArgumentException("Budget limit must be greater than 0");

            // Check for overlapping active budgets for the same user
            var hasOverlappingBudget = await _context.Budgets
                .AnyAsync(b => b.UserId == request.UserId &&
                              b.IsActive &&
                              !b.IsDelete &&
                              ((request.StartDate >= b.StartDate && request.StartDate <= b.EndDate) ||
                               (request.EndDate >= b.StartDate && request.EndDate <= b.EndDate)));

            if (hasOverlappingBudget)
                throw new InvalidOperationException("An active budget already exists for this time period");

            var budget = _mapper.Map<Budget>(request);
            budget.IsActive = true;
            budget.StartDate = request.StartDate ?? DateTime.UtcNow.AddHours(7);
            budget.IsDelete = false;

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return await GetBudgetByIdAsync(budget.Id);
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(int id, BudgetRequestModel request)
        {
            var budget = await _context.Budgets
                .Include(b => b.User)
                .Include(b => b.Alerts.Where(a => !a.IsDelete))
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            // Validate user exists and is not deleted
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDelete);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found or has been deleted");

            // Validate date range
            if (!IsValidDateRange(request.StartDate, request.EndDate))
                throw new ArgumentException("End date must be greater than or equal to start date");

            // Validate limit
            if (request.Limit <= 0)
                throw new ArgumentException("Budget limit must be greater than 0");

            // Check for overlapping active budgets for the same user (excluding current budget)
            var hasOverlappingBudget = await _context.Budgets
                .AnyAsync(b => b.Id != id &&
                              b.UserId == request.UserId &&
                              b.IsActive &&
                              !b.IsDelete &&
                              ((request.StartDate >= b.StartDate && request.StartDate <= b.EndDate) ||
                               (request.EndDate >= b.StartDate && request.EndDate <= b.EndDate)));

            if (hasOverlappingBudget)
                throw new InvalidOperationException("An active budget already exists for this time period");

            // Check if there are any expenses outside the new date range
            //if (request.StartDate.HasValue || request.EndDate.HasValue)
            //{
            //    var hasExpensesOutsideRange = await _context.Expenses
            //        .Include(e => e.Budget)
            //        .AnyAsync(e => e.Budget.Id == id &&
            //                      !e.IsDelete &&
            //                      ((request.StartDate.HasValue && e.Date < request.StartDate) ||
            //                       (request.EndDate.HasValue && e.Date > request.EndDate)));

            //    if (hasExpensesOutsideRange)
            //        throw new InvalidOperationException("Cannot update date range: There are expenses outside the new date range");
            //}

            _mapper.Map(request, budget);
            budget.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return await GetBudgetByIdAsync(budget.Id);
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Alerts)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            // Check if there are any active expenses
            if (budget.Expenses != null && budget.Expenses.Any(e => !e.IsDelete))
                throw new InvalidOperationException("Cannot delete budget: There are active expenses associated with it");

            // Soft delete the budget
            budget.IsDelete = true;
            budget.IsActive = false;
            budget.UpdatedDate = DateTime.UtcNow.AddHours(7);

            // Soft delete associated alerts
            if (budget.Alerts != null)
            {
                foreach (var alert in budget.Alerts.Where(a => !a.IsDelete))
                {
                    alert.IsDelete = true;
                    alert.UpdatedDate = DateTime.UtcNow.AddHours(7);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
