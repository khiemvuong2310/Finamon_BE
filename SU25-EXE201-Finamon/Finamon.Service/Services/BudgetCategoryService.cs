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

        public async Task<PaginatedResponse<BudgetCategoryResponse>> GetAllBudgetCategoriesAsync(BudgetCategoryQueryRequest queryRequest)
        {
            var query = _context.BudgetCategories
                .Include(bc => bc.Category)
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
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                query = queryRequest.SortBy.ToLower() switch
                {
                    "maxamount" => queryRequest.SortDescending
                        ? query.OrderByDescending(bc => bc.MaxAmount)
                        : query.OrderBy(bc => bc.MaxAmount),
                    "createdate" => queryRequest.SortDescending
                        ? query.OrderByDescending(bc => bc.CreatedAt)
                        : query.OrderBy(bc => bc.CreatedAt),
                    _ => query.OrderByDescending(bc => bc.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(bc => bc.Id); // Default sort
            }

            var paginatedCategories = await PaginatedResponse<BudgetCategory>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var categoryResponses = _mapper.Map<List<BudgetCategoryResponse>>(paginatedCategories.Items);
            
            return new PaginatedResponse<BudgetCategoryResponse>(categoryResponses, paginatedCategories.TotalCount, paginatedCategories.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetCategoryResponse> GetBudgetCategoryByIdAsync(int id)
        {
            var budgetCategory = await _context.BudgetCategories
                .Include(bc => bc.Category)
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);

            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            return _mapper.Map<BudgetCategoryResponse>(budgetCategory);
        }

        public async Task<BudgetCategoryResponse> CreateBudgetCategoryAsync(BudgetCategoryRequestModel request)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");

            var budgetCategory = _mapper.Map<BudgetCategory>(request);
            budgetCategory.CreatedAt = DateTime.UtcNow.AddHours(7);
            budgetCategory.IsDelete = false;

            _context.BudgetCategories.Add(budgetCategory);
            await _context.SaveChangesAsync();

            return await GetBudgetCategoryByIdAsync(budgetCategory.Id);
        }

        public async Task<BudgetCategoryResponse> UpdateBudgetCategoryAsync(int id, BudgetCategoryRequestModel request)
        {
            var budgetCategory = await _context.BudgetCategories
                .Include(bc => bc.Category)
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);

            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            if (request.CategoryId != budgetCategory.CategoryId)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
                if (category == null)
                    throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");
            }

            _mapper.Map(request, budgetCategory);
            budgetCategory.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return await GetBudgetCategoryByIdAsync(budgetCategory.Id);
        }

        public async Task DeleteBudgetCategoryAsync(int id)
        {
            var budgetCategory = await _context.BudgetCategories.FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDelete);
            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {id} not found or has been deleted");

            budgetCategory.IsDelete = true;
            budgetCategory.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<BudgetCategoryResponse>> GetBudgetCategoriesByCategoryIdAsync(int categoryId, BudgetCategoryQueryRequest queryRequest)
        {
            queryRequest.CategoryId = categoryId;
            return await GetAllBudgetCategoriesAsync(queryRequest);
        }
    }
} 