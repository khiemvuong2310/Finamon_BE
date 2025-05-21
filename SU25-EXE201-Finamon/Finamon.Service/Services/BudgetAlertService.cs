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

        public async Task<IEnumerable<BudgetAlertResponse>> GetAllAlertsAsync(BudgetAlertQueryRequest query)
        {
            var alerts = _context.BudgetAlerts
                .Include(a => a.Budget)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Message))
            {
                alerts = alerts.Where(a => a.Message.Contains(query.Message));
            }

            if (query.BudgetId.HasValue)
            {
                alerts = alerts.Where(a => a.BudgetId == query.BudgetId.Value);
            }

            if (query.CreatedFrom.HasValue)
            {
                alerts = alerts.Where(a => a.CreatedAt >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                alerts = alerts.Where(a => a.CreatedAt <= query.CreatedTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                alerts = query.SortBy.ToLower() switch
                {
                    "message" => query.SortDescending
                        ? alerts.OrderByDescending(a => a.Message)
                        : alerts.OrderBy(a => a.Message),
                    "createat" => query.SortDescending
                        ? alerts.OrderByDescending(a => a.CreatedAt)
                        : alerts.OrderBy(a => a.CreatedAt),
                    "budgetid" => query.SortDescending
                        ? alerts.OrderByDescending(a => a.BudgetId)
                        : alerts.OrderBy(a => a.BudgetId),
                    _ => alerts
                };
            }

            // Apply pagination
            if (query.PageSize > 0)
            {
                alerts = alerts
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            var result = await alerts.ToListAsync();
            return _mapper.Map<IEnumerable<BudgetAlertResponse>>(result);
        }

        public async Task<BudgetAlertResponse> GetAlertByIdAsync(int id)
        {
            var alert = await _context.BudgetAlerts
                .Include(a => a.Budget)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found");

            return _mapper.Map<BudgetAlertResponse>(alert);
        }

        public async Task<IEnumerable<BudgetAlertResponse>> GetAlertsByBudgetIdAsync(int budgetId)
        {
            var alerts = await _context.BudgetAlerts
                .Include(a => a.Budget)
                .Where(a => a.BudgetId == budgetId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<BudgetAlertResponse>>(alerts);
        }

        public async Task<BudgetAlertResponse> CreateAlertAsync(BudgetAlertRequestModel request)
        {
            var alert = _mapper.Map<BudgetAlert>(request);
            alert.CreatedAt = DateTime.UtcNow.AddHours(7);

            _context.BudgetAlerts.Add(alert);
            await _context.SaveChangesAsync();

            return _mapper.Map<BudgetAlertResponse>(alert);
        }

        public async Task<BudgetAlertResponse> UpdateAlertAsync(int id, BudgetAlertRequestModel request)
        {
            var alert = await _context.BudgetAlerts.FindAsync(id);
            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found");

            _mapper.Map(request, alert);
            alert.UpdatedDate = DateTime.UtcNow.AddHours(7);  

            await _context.SaveChangesAsync();

            return _mapper.Map<BudgetAlertResponse>(alert);
        }

        public async Task DeleteAlertAsync(int id)
        {
            var alert = await _context.BudgetAlerts.FindAsync(id);
            if (alert == null)
                throw new KeyNotFoundException($"Alert with ID {id} not found");

            _context.BudgetAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
} 