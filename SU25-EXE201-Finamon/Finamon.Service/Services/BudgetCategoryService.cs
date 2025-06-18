using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{
    public class BudgetCategoryService : IBudgetCategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BudgetCategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<decimal> CalculateCurrentAmount(int categoryId, decimal maxAmount)
        {
            var totalExpenses = await _context.Expenses
                .Where(e => e.CategoryId == categoryId && !e.IsDelete)
                .SumAsync(e => e.Amount);

            // Return remaining amount (maxAmount - total expenses)
            return maxAmount - totalExpenses;
        }

        public async Task<PaginatedResponse<BudgetCategoryResponse>> GetAllBudgetCategoriesAsync(BudgetCategoryQueryRequest queryRequest)
        {
            var query = _context.BudgetCategories
                .Include(bc => bc.Category)
                .Include(bc => bc.CategoryAlerts.Where(ca => !ca.IsDelete))
                .AsQueryable();

            // Default to not showing deleted if not specified in the query
            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(bc => bc.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(bc => !bc.IsDelete);
            }

            // Apply filters
            if (queryRequest.CategoryId.HasValue)
            {
                query = query.Where(bc => bc.CategoryId == queryRequest.CategoryId.Value);
            }

            if (queryRequest.MinMaxAmount.HasValue)
            {
                query = query.Where(bc => bc.MaxAmount >= queryRequest.MinMaxAmount.Value);
            }

            if (queryRequest.MaxMaxAmount.HasValue)
            {
                query = query.Where(bc => bc.MaxAmount <= queryRequest.MaxMaxAmount.Value);
            }

            if (queryRequest.CreatedFrom.HasValue)
            {
                query = query.Where(bc => bc.CreatedAt >= queryRequest.CreatedFrom.Value);
            }

            if (queryRequest.CreatedTo.HasValue)
            {
                query = query.Where(bc => bc.CreatedAt <= queryRequest.CreatedTo.Value);
            }

            // Apply sorting
            if (queryRequest.SortBy.HasValue)
            {
                query = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? query.OrderByDescending(bc => bc.CreatedAt)
                        : query.OrderBy(bc => bc.CreatedAt),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? query.OrderByDescending(bc => bc.UpdatedDate)
                        : query.OrderBy(bc => bc.UpdatedDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? query.OrderByDescending(bc => bc.MaxAmount)
                        : query.OrderBy(bc => bc.MaxAmount),
                    _ => query.OrderByDescending(bc => bc.Id) 
                };
            }
            else
            {
                query = query.OrderByDescending(bc => bc.Id); 
            }

            var paginatedCategories = await PaginatedResponse<BudgetCategory>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var categoryResponses = _mapper.Map<List<BudgetCategoryResponse>>(paginatedCategories.Items);

            // Calculate CurrentAmount for each budget category
            foreach (var categoryResponse in categoryResponses)
            {
                categoryResponse.CurrentAmount = await CalculateCurrentAmount(categoryResponse.CategoryId, categoryResponse.MaxAmount);
            }
            
            return new PaginatedResponse<BudgetCategoryResponse>(categoryResponses, paginatedCategories.TotalCount, paginatedCategories.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetCategoryResponse> GetBudgetCategoryByIdAsync(int id)
        {
            var budgetCategory = await _context.BudgetCategories
                .Include(bc => bc.Category)
                .Include(bc => bc.CategoryAlerts.Where(ca => !ca.IsDelete))
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);

            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            var response = _mapper.Map<BudgetCategoryResponse>(budgetCategory);
            response.CurrentAmount = await CalculateCurrentAmount(response.CategoryId, response.MaxAmount);

            return response;
        }

        public async Task<BudgetCategoryResponse> CreateBudgetCategoryAsync(BudgetCategoryRequestModel request)
        {
            // Validate category exists and is not deleted
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");

            // Validate MaxAmount
            if (request.MaxAmount <= 0)
                throw new ArgumentException("MaxAmount must be greater than 0");

            // Check if category already has an active budget
            var existingBudget = await _context.BudgetCategories
                .AnyAsync(bc => bc.CategoryId == request.CategoryId && !bc.IsDelete);
            if (existingBudget)
                throw new InvalidOperationException($"Category with ID {request.CategoryId} already has an active budget");

            var budgetCategory = _mapper.Map<BudgetCategory>(request);
            budgetCategory.CreatedAt = DateTime.UtcNow.AddHours(7);
            budgetCategory.IsDelete = false;
            budgetCategory.CategoryAlerts = new List<CategoryAlert>();

            _context.BudgetCategories.Add(budgetCategory);
            await _context.SaveChangesAsync();

            return await GetBudgetCategoryByIdAsync(budgetCategory.Id);
        }

        public async Task<BudgetCategoryResponse> UpdateBudgetCategoryAsync(int id, BudgetCategoryRequestModel request)
        {
            var budgetCategory = await _context.BudgetCategories
                .Include(bc => bc.Category)
                .Include(bc => bc.CategoryAlerts.Where(ca => !ca.IsDelete))
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);

            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            // Validate MaxAmount
            if (request.MaxAmount <= 0)
                throw new ArgumentException("MaxAmount must be greater than 0");

            // Check if current expenses exceed new MaxAmount
            var currentExpenses = await _context.Expenses
                .Where(e => e.CategoryId == budgetCategory.CategoryId && !e.IsDelete)
                .SumAsync(e => e.Amount);

            if (currentExpenses > request.MaxAmount)
                throw new InvalidOperationException($"Cannot update MaxAmount to {request.MaxAmount} as current expenses ({currentExpenses}) exceed this amount");

            // If category is being changed
            if (request.CategoryId != budgetCategory.CategoryId)
            {
                // Validate new category exists and is not deleted
                var newCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
                if (newCategory == null)
                    throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");

                // Check if new category already has an active budget
                var existingBudget = await _context.BudgetCategories
                    .AnyAsync(bc => bc.CategoryId == request.CategoryId && bc.Id != id && !bc.IsDelete);
                if (existingBudget)
                    throw new InvalidOperationException($"Category with ID {request.CategoryId} already has an active budget");
            }

            _mapper.Map(request, budgetCategory);
            budgetCategory.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return await GetBudgetCategoryByIdAsync(budgetCategory.Id);
        }

        public async Task DeleteBudgetCategoryAsync(int id)
        {
            var budgetCategory = await _context.BudgetCategories
                .Include(bc => bc.CategoryAlerts)
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);

            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            // Check if there are any expenses for this category in the current month
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var hasCurrentExpenses = await _context.Expenses
                .AnyAsync(e => e.CategoryId == budgetCategory.CategoryId && 
                              !e.IsDelete &&
                              e.Date >= startOfMonth &&
                              e.Date <= endOfMonth);

            if (hasCurrentExpenses)
                throw new InvalidOperationException("Cannot delete budget category as it has expenses in the current month");

            // Soft delete the budget category
            budgetCategory.IsDelete = true;
            budgetCategory.UpdatedDate = DateTime.UtcNow.AddHours(7);

            // Soft delete associated alerts
            if (budgetCategory.CategoryAlerts != null)
            {
                foreach (var alert in budgetCategory.CategoryAlerts.Where(a => !a.IsDelete))
                {
                    alert.IsDelete = true;
                    alert.UpdatedDate = DateTime.UtcNow.AddHours(7);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<BudgetCategoryResponse>> GetBudgetCategoriesByCategoryIdAsync(int categoryId, BudgetCategoryQueryRequest queryRequest)
        {
            queryRequest.CategoryId = categoryId;
            return await GetAllBudgetCategoriesAsync(queryRequest);
        }
    }
} 