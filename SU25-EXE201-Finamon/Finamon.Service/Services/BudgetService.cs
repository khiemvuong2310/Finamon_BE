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

        public async Task<IEnumerable<BudgetResponse>> GetAllBudgetsAsync(BudgetQueryRequest? queryRequest = null)
        {
            var query = _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts)
                .AsQueryable();

            if (queryRequest != null)
            {
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

            var budgets = await query.ToListAsync();
            return _mapper.Map<IEnumerable<BudgetResponse>>(budgets);
        }

        public async Task<BudgetResponse> GetBudgetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Expenses)
                .Include(b => b.Alerts)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found");

            return _mapper.Map<BudgetResponse>(budget);
        }

        public async Task<BudgetResponse> CreateBudgetAsync(BudgetRequestModel request)
        {
            var budget = _mapper.Map<Budget>(request);
            budget.StartDate = DateTime.UtcNow;

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return _mapper.Map<BudgetResponse>(budget);
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(int id, BudgetRequestModel request)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found");

            _mapper.Map(request, budget);
            budget.EndDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<BudgetResponse>(budget);
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found");

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}
