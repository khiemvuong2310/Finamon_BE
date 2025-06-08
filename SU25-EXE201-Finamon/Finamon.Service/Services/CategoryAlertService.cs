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
    public class CategoryAlertService : ICategoryAlertService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryAlertService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<CategoryAlertResponse>> GetAllCategoryAlertsAsync(CategoryAlertQueryRequest queryRequest)
        {
            var query = _context.CategoryAlerts
                .Include(ca => ca.BudgetCategory)
                .AsQueryable();

            // Default to not showing deleted if not specified in the query
            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(ca => ca.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(ca => !ca.IsDelete);
            }

            // Apply filters
            if (queryRequest.BudgetCategoryId.HasValue)
            {
                query = query.Where(ca => ca.BudgetCategoryId == queryRequest.BudgetCategoryId.Value);
            }

            if (queryRequest.MinThreshold.HasValue)
            {
                query = query.Where(ca => ca.AlertThreshold >= queryRequest.MinThreshold.Value);
            }

            if (queryRequest.MaxThreshold.HasValue)
            {
                query = query.Where(ca => ca.AlertThreshold <= queryRequest.MaxThreshold.Value);
            }

            if (queryRequest.CreatedFrom.HasValue)
            {
                query = query.Where(ca => ca.CreatedDate >= queryRequest.CreatedFrom.Value);
            }

            if (queryRequest.CreatedTo.HasValue)
            {
                query = query.Where(ca => ca.CreatedDate <= queryRequest.CreatedTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                query = queryRequest.SortBy.ToLower() switch
                {
                    "threshold" => queryRequest.SortDescending
                        ? query.OrderByDescending(ca => ca.AlertThreshold)
                        : query.OrderBy(ca => ca.AlertThreshold),
                    "createdate" => queryRequest.SortDescending
                        ? query.OrderByDescending(ca => ca.CreatedDate)
                        : query.OrderBy(ca => ca.CreatedDate),
                    _ => query.OrderByDescending(ca => ca.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(ca => ca.Id); // Default sort
            }

            var paginatedAlerts = await PaginatedResponse<CategoryAlert>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var alertResponses = _mapper.Map<List<CategoryAlertResponse>>(paginatedAlerts.Items);
            
            return new PaginatedResponse<CategoryAlertResponse>(alertResponses, paginatedAlerts.TotalCount, paginatedAlerts.PageIndex, queryRequest.PageSize);
        }

        public async Task<CategoryAlertResponse> GetCategoryAlertByIdAsync(int id)
        {
            var categoryAlert = await _context.CategoryAlerts
                .Include(ca => ca.BudgetCategory)
                .FirstOrDefaultAsync(ca => ca.Id == id && !ca.IsDelete);

            if (categoryAlert == null)
                throw new KeyNotFoundException($"CategoryAlert with ID {id} not found or has been deleted");

            return _mapper.Map<CategoryAlertResponse>(categoryAlert);
        }

        public async Task<CategoryAlertResponse> CreateCategoryAlertAsync(CategoryAlertRequestModel request)
        {
            var budgetCategory = await _context.BudgetCategories
                .FirstOrDefaultAsync(bc => bc.Id == request.BudgetCategoryId && !bc.IsDelete);
            
            if (budgetCategory == null)
                throw new KeyNotFoundException($"BudgetCategory with ID {request.BudgetCategoryId} not found or has been deleted");

            var categoryAlert = _mapper.Map<CategoryAlert>(request);
            categoryAlert.CreatedDate = DateTime.UtcNow.AddHours(7);
            categoryAlert.IsDelete = false;

            _context.CategoryAlerts.Add(categoryAlert);
            await _context.SaveChangesAsync();

            return await GetCategoryAlertByIdAsync(categoryAlert.Id);
        }

        public async Task<CategoryAlertResponse> UpdateCategoryAlertAsync(int id, CategoryAlertRequestModel request)
        {
            var categoryAlert = await _context.CategoryAlerts
                .Include(ca => ca.BudgetCategory)
                .FirstOrDefaultAsync(ca => ca.Id == id && !ca.IsDelete);

            if (categoryAlert == null)
                throw new KeyNotFoundException($"CategoryAlert with ID {id} not found or has been deleted");

            if (request.BudgetCategoryId != categoryAlert.BudgetCategoryId)
            {
                var budgetCategory = await _context.BudgetCategories
                    .FirstOrDefaultAsync(bc => bc.Id == request.BudgetCategoryId && !bc.IsDelete);
                
                if (budgetCategory == null)
                    throw new KeyNotFoundException($"BudgetCategory with ID {request.BudgetCategoryId} not found or has been deleted");
            }

            _mapper.Map(request, categoryAlert);
            categoryAlert.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return await GetCategoryAlertByIdAsync(categoryAlert.Id);
        }

        public async Task DeleteCategoryAlertAsync(int id)
        {
            var categoryAlert = await _context.CategoryAlerts
                .FirstOrDefaultAsync(ca => ca.Id == id && !ca.IsDelete);

            if (categoryAlert == null)
                throw new KeyNotFoundException($"CategoryAlert with ID {id} not found or has been deleted");

            categoryAlert.IsDelete = true;
            categoryAlert.UpdatedDate = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<CategoryAlertResponse>> GetAlertsByBudgetCategoryIdAsync(int budgetCategoryId, CategoryAlertQueryRequest queryRequest)
        {
            queryRequest.BudgetCategoryId = budgetCategoryId;
            return await GetAllCategoryAlertsAsync(queryRequest);
        }
    }
} 