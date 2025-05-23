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
    public class BudgetAlertService : IBudgetAlertService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BudgetAlertService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<BudgetAlertResponse>> GetAllAlertsAsync(BudgetAlertQueryRequest queryRequest)
        {
            var query = _context.BudgetAlerts
                .Include(a => a.Budget)
                .AsQueryable();

            // Default to not showing deleted if not specified in the query
            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(a => a.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(a => !a.IsDelete);
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(queryRequest.Message))
            {
                query = query.Where(a => a.Message.Contains(queryRequest.Message));
            }

            if (queryRequest.BudgetId.HasValue)
            {
                query = query.Where(a => a.BudgetId == queryRequest.BudgetId.Value);
            }

            if (queryRequest.CreatedFrom.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= queryRequest.CreatedFrom.Value);
            }

            if (queryRequest.CreatedTo.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= queryRequest.CreatedTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                query = queryRequest.SortBy.ToLower() switch
                {
                    "message" => queryRequest.SortDescending
                        ? query.OrderByDescending(a => a.Message)
                        : query.OrderBy(a => a.Message),
                    "createdat" => queryRequest.SortDescending  // Corrected from "createat"
                        ? query.OrderByDescending(a => a.CreatedAt)
                        : query.OrderBy(a => a.CreatedAt),
                    "budgetid" => queryRequest.SortDescending
                        ? query.OrderByDescending(a => a.BudgetId)
                        : query.OrderBy(a => a.BudgetId),
                    _ => query.OrderByDescending(a => a.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(a => a.Id); // Default sort
            }

            var paginatedAlerts = await PaginatedResponse<BudgetAlert>.CreateAsync(query, queryRequest.PageNumber, queryRequest.PageSize);
            var alertResponses = _mapper.Map<List<BudgetAlertResponse>>(paginatedAlerts.Items);

            return new PaginatedResponse<BudgetAlertResponse>(alertResponses, paginatedAlerts.TotalCount, paginatedAlerts.PageIndex, queryRequest.PageSize);
        }

        public async Task<BudgetAlertResponse> GetAlertByIdAsync(int id)
        {
            var alert = await _context.BudgetAlerts
                .Include(a => a.Budget)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDelete);

            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found or has been deleted");

            return _mapper.Map<BudgetAlertResponse>(alert);
        }

        public async Task<PaginatedResponse<BudgetAlertResponse>> GetAlertsByBudgetIdAsync(int budgetId, BudgetAlertQueryRequest queryRequest)
        {
            queryRequest.BudgetId = budgetId; // Ensure BudgetId is set for filtering
            // queryRequest.IsDeleted = false; // Ensure only non-deleted alerts are fetched by default if not specified
            return await GetAllAlertsAsync(queryRequest);
        }

        public async Task<BudgetAlertResponse> CreateAlertAsync(BudgetAlertRequestModel request)
        {
            var budgetExists = await _context.Budgets.AnyAsync(b => b.Id == request.BudgetId && !b.IsDelete);
            if (!budgetExists) throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found or has been deleted.");

            var alert = _mapper.Map<BudgetAlert>(request);
            alert.CreatedAt = DateTime.UtcNow;
            alert.IsDelete = false;

            _context.BudgetAlerts.Add(alert);
            await _context.SaveChangesAsync();

            // Re-fetch to include Budget for the response
            var createdAlert = await _context.BudgetAlerts.Include(a => a.Budget).FirstOrDefaultAsync(a => a.Id == alert.Id);
            return _mapper.Map<BudgetAlertResponse>(createdAlert);
        }

        public async Task<BudgetAlertResponse> UpdateAlertAsync(int id, BudgetAlertRequestModel request)
        {
            var alert = await _context.BudgetAlerts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDelete);
            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found or has been deleted");
            
            var budgetExists = await _context.Budgets.AnyAsync(b => b.Id == request.BudgetId && !b.IsDelete);
            if (!budgetExists) throw new KeyNotFoundException($"Budget with ID {request.BudgetId} not found or has been deleted.");

            _mapper.Map(request, alert);
            alert.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            var updatedAlert = await _context.BudgetAlerts.Include(a => a.Budget).FirstOrDefaultAsync(a => a.Id == alert.Id);
            return _mapper.Map<BudgetAlertResponse>(updatedAlert);
        }

        public async Task DeleteAlertAsync(int id)
        {
            var alert = await _context.BudgetAlerts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDelete);
            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found or has been deleted");

            alert.IsDelete = true;
            alert.UpdatedDate = DateTime.UtcNow;
            //_context.BudgetAlerts.Remove(alert); // Soft delete
            await _context.SaveChangesAsync();
        }
    }
} 