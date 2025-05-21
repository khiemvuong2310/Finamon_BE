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

        public async Task<IEnumerable<BudgetDetailResponse>> GetAllBudgetDetailsAsync(BudgetDetailQueryRequest queryRequest = null)
        {
            var query = _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .AsQueryable();

            if (queryRequest != null)
            {
                // Apply filters
                if (queryRequest.BudgetId.HasValue)
                {
                    query = query.Where(bd => bd.BudgetId == queryRequest.BudgetId.Value);
                }

                if (queryRequest.CategoryId.HasValue)
                {
                    query = query.Where(bd => bd.CategoryId == queryRequest.CategoryId.Value);
                }

                if (queryRequest.MinMaxAmount.HasValue)
                {
                    query = query.Where(bd => bd.MaxAmount >= queryRequest.MinMaxAmount.Value);
                }

                if (queryRequest.MaxMaxAmount.HasValue)
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
                        _ => query
                    };
                }

                // Apply pagination
                if (queryRequest.PageSize > 0)
                {
                    query = query.Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
                               .Take(queryRequest.PageSize);
                }
            }

            var budgetDetails = await query.ToListAsync();
            return _mapper.Map<IEnumerable<BudgetDetailResponse>>(budgetDetails);
        }

        public async Task<BudgetDetailResponse> GetBudgetDetailByIdAsync(int id)
        {
            var budgetDetail = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .FirstOrDefaultAsync(bd => bd.Id == id);

            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found");

            return _mapper.Map<BudgetDetailResponse>(budgetDetail);
        }

        public async Task<BudgetDetailResponse> CreateBudgetDetailAsync(BudgetDetailRequestModel request)
        {
            // Validate Budget exists
            var budget = await _context.Budgets.FindAsync(request.BudgetId);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found");

            // Validate Category exists
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            var budgetDetail = _mapper.Map<BudgetDetail>(request);
            budgetDetail.CreatedAt = DateTime.UtcNow.AddHours(7);
            budgetDetail.CurrentAmount = 0; // Initialize current amount to 0

            _context.BudgetDetails.Add(budgetDetail);
            await _context.SaveChangesAsync();

            return await GetBudgetDetailByIdAsync(budgetDetail.Id);
        }

        public async Task<BudgetDetailResponse> UpdateBudgetDetailAsync(int id, BudgetDetailRequestModel request)
        {
            var budgetDetail = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .FirstOrDefaultAsync(bd => bd.Id == id);

            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found");

            // Validate Budget exists if changed
            if (request.BudgetId != budgetDetail.BudgetId)
            {
                var budget = await _context.Budgets.FindAsync(request.BudgetId);
                if (budget == null)
                    throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found");
            }

            // Validate Category exists if changed
            if (request.CategoryId != budgetDetail.CategoryId)
            {
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                    throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");
            }

            _mapper.Map(request, budgetDetail);
            budgetDetail.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return await GetBudgetDetailByIdAsync(budgetDetail.Id);
        }

        public async Task DeleteBudgetDetailAsync(int id)
        {
            var budgetDetail = await _context.BudgetDetails.FindAsync(id);
            if (budgetDetail == null)
                throw new KeyNotFoundException($"BudgetDetail with ID {id} not found");

            _context.BudgetDetails.Remove(budgetDetail);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BudgetDetailResponse>> GetBudgetDetailsByBudgetIdAsync(int budgetId)
        {
            var budgetDetails = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .Where(bd => bd.BudgetId == budgetId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDetailResponse>>(budgetDetails);
        }

        public async Task<IEnumerable<BudgetDetailResponse>> GetBudgetDetailsByCategoryIdAsync(int categoryId)
        {
            var budgetDetails = await _context.BudgetDetails
                .Include(bd => bd.Budget)
                .Include(bd => bd.Category)
                .Where(bd => bd.CategoryId == categoryId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDetailResponse>>(budgetDetails);
        }
    }
} 