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

        public async Task<PaginatedResponse<BudgetResponse>> GetAllBudgetsAsync(BudgetQueryRequest queryRequest)
        {
            var query = _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts)
                .Include(b => b.BudgetDetails)
                    .ThenInclude(bd => bd.Category)
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
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                query = queryRequest.SortBy.ToLower() switch
                {
                    "limit" => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.Limit)
                        : query.OrderBy(b => b.Limit),
                    "startdate" => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.StartDate)
                        : query.OrderBy(b => b.StartDate),
                    "enddate" => queryRequest.SortDescending
                        ? query.OrderByDescending(b => b.EndDate)
                        : query.OrderBy(b => b.EndDate),
                    _ => query.OrderByDescending(b => b.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.Id); // Default sort
            }

            var paginatedBudgets = await PaginatedResponse<Budget>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var budgetResponses = _mapper.Map<List<BudgetResponse>>(paginatedBudgets.Items);
            
            return new PaginatedResponse<BudgetResponse>(budgetResponses, paginatedBudgets.TotalCount, paginatedBudgets.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetResponse> GetBudgetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts)
                .Include(b => b.BudgetDetails)
                    .ThenInclude(bd => bd.Category)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete); // Ensure not deleted

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            return _mapper.Map<BudgetResponse>(budget);
        }

        public async Task<BudgetResponse> CreateBudgetAsync(BudgetRequestModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDelete);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found or has been deleted");

            var budget = _mapper.Map<Budget>(request);
            budget.CurrentAmount = 0; // Set mặc định là 0
            budget.IsActive = true;   // Set mặc định là active
            budget.StartDate = request.StartDate ?? DateTime.UtcNow; 
            budget.IsDelete = false; // Set IsDelete to false on creation

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return await GetBudgetByIdAsync(budget.Id);
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(int id, BudgetRequestModel request)
        {
            var budget = await _context.Budgets
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDelete);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found or has been deleted");

            _mapper.Map(request, budget);
            budget.UpdatedDate = DateTime.UtcNow; // Assuming Budget entity has UpdatedDate

            await _context.SaveChangesAsync();

            return await GetBudgetByIdAsync(budget.Id);
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found or has been deleted");

            budget.IsDelete = true;
            budget.UpdatedDate = DateTime.UtcNow; // Assuming Budget entity has UpdatedDate
            //_context.Budgets.Remove(budget); // Soft delete instead of hard delete
            await _context.SaveChangesAsync();
        }
    }
}
