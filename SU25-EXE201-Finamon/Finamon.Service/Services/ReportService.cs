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
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReportService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReportResponse>> GetAllReportsAsync(ReportQueryRequest query)
        {
            var reports = _context.Reports
                .Include(r => r.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                reports = reports.Where(r => r.Title.Contains(query.Title));
            }

            if (query.UserId.HasValue)
            {
                reports = reports.Where(r => r.UserId == query.UserId.Value);
            }

            if (query.CreatedFrom.HasValue)
            {
                reports = reports.Where(r => r.CreatedDate >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                reports = reports.Where(r => r.CreatedDate <= query.CreatedTo.Value);
            }

            // Apply sorting
            if (query.SortBy.HasValue)
            {
                reports = query.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => query.SortDescending
                        ? reports.OrderByDescending(r => r.CreatedDate)
                        : reports.OrderBy(r => r.CreatedDate),
                    SortByEnum.UpdatedDate => query.SortDescending
                        ? reports.OrderByDescending(r => r.UpdatedDate)
                        : reports.OrderBy(r => r.UpdatedDate),
                    _ => reports.OrderByDescending(r => r.CreatedDate)
                };
            }

            // Apply pagination
            if (query.PageSize > 0)
            {
                reports = reports
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            var result = await reports.ToListAsync();
            return _mapper.Map<IEnumerable<ReportResponse>>(result);
        }

        public async Task<ReportResponse> GetReportByIdAsync(int id)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                throw new KeyNotFoundException($"Report with ID {id} not found");

            return _mapper.Map<ReportResponse>(report);
        }

        public async Task<ReportResponse> CreateReportAsync(ReportRequestModel request)
        {
            var report = _mapper.Map<Report>(request);
            report.CreatedDate = DateTime.UtcNow;

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return await GetReportByIdAsync(report.Id);
        }

        public async Task<ReportResponse> UpdateReportAsync(int id, ReportRequestModel request)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {id} not found");

            _mapper.Map(request, report);
            report.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetReportByIdAsync(id);
        }

        public async Task DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {id} not found");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
} 