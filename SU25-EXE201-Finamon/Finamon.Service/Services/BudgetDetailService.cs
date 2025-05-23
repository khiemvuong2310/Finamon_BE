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
    public class BudgetDetailService : IBudgetDetailService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BudgetDetailService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<BudgetDetailResponse>> GetAllBudgetDetailsAsync(BudgetDetailQueryRequest queryRequest)
        {
            var query = _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .AsQueryable();

            // Default to not showing deleted if not specified in the query
            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(bd => bd.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(bd => !bd.IsDelete);
            }

            // Apply filters
            if (queryRequest.BudgetId.HasValue)
            {
                query = query.Where(bd => bd.BudgetId == queryRequest.BudgetId.Value);
            }

            if (queryRequest.CategoryId.HasValue)
            {
                query = query.Where(bd => bd.CategoryId == queryRequest.CategoryId.Value);
            }

            if (queryRequest.MinMaxAmount.HasValue) // Assuming this filters by MaxAmount >= value
            {
                query = query.Where(bd => bd.MaxAmount >= queryRequest.MinMaxAmount.Value);
            }

            if (queryRequest.MaxMaxAmount.HasValue) // Assuming this filters by MaxAmount <= value
            {
                query = query.Where(bd => bd.MaxAmount <= queryRequest.MaxMaxAmount.Value);
            }

            if (queryRequest.MinCurrentAmount.HasValue)
            {
                query = query.Where(bd => bd.CurrentAmount >= queryRequest.MinCurrentAmount.Value);
            }

            if (queryRequest.MaxCurrentAmount.HasValue)
            {
                query = query.Where(bd => bd.CurrentAmount <= queryRequest.MaxCurrentAmount.Value);
            }

            if (queryRequest.CreatedFrom.HasValue)
            {
                query = query.Where(bd => bd.CreatedAt >= queryRequest.CreatedFrom.Value);
            }

            if (queryRequest.CreatedTo.HasValue)
            {
                query = query.Where(bd => bd.CreatedAt <= queryRequest.CreatedTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                query = queryRequest.SortBy.ToLower() switch
                {
                    "maxamount" => queryRequest.SortDescending
                        ? query.OrderByDescending(bd => bd.MaxAmount)
                        : query.OrderBy(bd => bd.MaxAmount),
                    "currentamount" => queryRequest.SortDescending
                        ? query.OrderByDescending(bd => bd.CurrentAmount)
                        : query.OrderBy(bd => bd.CurrentAmount),
                    "createdat" => queryRequest.SortDescending
                        ? query.OrderByDescending(bd => bd.CreatedAt)
                        : query.OrderBy(bd => bd.CreatedAt),
                    _ => query.OrderByDescending(bd => bd.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(bd => bd.Id); // Default sort
            }

            var paginatedDetails = await PaginatedResponse<BudgetDetail>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var detailResponses = _mapper.Map<List<BudgetDetailResponse>>(paginatedDetails.Items);
            
            return new PaginatedResponse<BudgetDetailResponse>(detailResponses, paginatedDetails.TotalCount, paginatedDetails.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetDetailResponse> GetBudgetDetailByIdAsync(int id)
        {
            var budgetDetail = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .FirstOrDefaultAsync(bd => bd.Id == id && !bd.IsDelete);

            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found or has been deleted");

            return _mapper.Map<BudgetDetailResponse>(budgetDetail);
        }

        public async Task<BudgetDetailResponse> CreateBudgetDetailAsync(BudgetDetailRequestModel request)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == request.BudgetId && !b.IsDelete);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found or has been deleted");

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");

            var budgetDetail = _mapper.Map<BudgetDetail>(request);
            budgetDetail.CreatedAt = DateTime.UtcNow;
            budgetDetail.CurrentAmount = 0; 
            budgetDetail.IsDelete = false;

            _context.BudgetDetails.Add(budgetDetail);
            await _context.SaveChangesAsync();

            return await GetBudgetDetailByIdAsync(budgetDetail.Id);
        }

        public async Task<BudgetDetailResponse> UpdateBudgetDetailAsync(int id, BudgetDetailRequestModel request)
        {
            var budgetDetail = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .FirstOrDefaultAsync(bd => bd.Id == id && !bd.IsDelete);

            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found or has been deleted");

            if (request.BudgetId != budgetDetail.BudgetId)
            {
                var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == request.BudgetId && !b.IsDelete);
                if (budget == null)
                    throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found or has been deleted");
            }

            if (request.CategoryId != budgetDetail.CategoryId)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
                if (category == null)
                    throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");
            }

            _mapper.Map(request, budgetDetail);
            budgetDetail.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetBudgetDetailByIdAsync(budgetDetail.Id);
        }

        public async Task DeleteBudgetDetailAsync(int id)
        {
            var budgetDetail = await _context.BudgetDetails.FirstOrDefaultAsync(bd => bd.Id == id && !bd.IsDelete);
            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found or has been deleted");

            budgetDetail.IsDelete = true;
            budgetDetail.UpdatedDate = DateTime.UtcNow;
            //_context.BudgetDetails.Remove(budgetDetail); // Soft delete
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<BudgetDetailResponse>> GetBudgetDetailsByBudgetIdAsync(int budgetId, BudgetDetailQueryRequest queryRequest)
        {
            queryRequest.BudgetId = budgetId; 
            // queryRequest.IsDeleted = false; // Ensure only non-deleted are fetched by default
            return await GetAllBudgetDetailsAsync(queryRequest);
        }

        public async Task<PaginatedResponse<BudgetDetailResponse>> GetBudgetDetailsByCategoryIdAsync(int categoryId, BudgetDetailQueryRequest queryRequest)
        {
            queryRequest.CategoryId = categoryId;
            // queryRequest.IsDeleted = false; // Ensure only non-deleted are fetched by default
            return await GetAllBudgetDetailsAsync(queryRequest);
        }
    }
} 