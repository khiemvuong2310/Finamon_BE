using AutoMapper;
using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Finamon_Data.Entities;
using Finamon.Repo.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Finamon.Service.Services
{
    public class SiteAnalyticService : ISiteAnalyticService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SiteAnalyticService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<SiteAnalyticResponse>> GetAllAsync(SiteAnalyticQueryRequest request)
        {
            var query = _unitOfWork.Repository<SiteAnalytic>().GetAll();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SiteName))
                query = query.Where(x => x.SiteName.Contains(request.SiteName));

            if (request.MinCount.HasValue)
                query = query.Where(x => x.Count >= request.MinCount);

            if (request.MaxCount.HasValue)
                query = query.Where(x => x.Count <= request.MaxCount);

            if (request.FromDate.HasValue)
                query = query.Where(x => x.CreatedDate >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.CreatedDate <= request.ToDate);

            // Apply sorting
            query = request.SortBy switch
            {
                SortByEnum.CreatedDate => query.OrderByDescending(x => x.CreatedDate),
                SortByEnum.UpdatedDate => query.OrderBy(x => x.UpdatedDate),
                _ => query.OrderByDescending(x => x.CreatedDate)
            };

            // Apply pagination and map to response
            var paginatedItems = await PaginatedResponse<SiteAnalyticResponse>.CreateAsync(
                query.Select(x => _mapper.Map<SiteAnalyticResponse>(x)),
                request.PageNumber,
                request.PageSize
            );

            return paginatedItems;
        }

        public async Task<SiteAnalyticResponse> GetByIdAsync(int id)
        {
            var analytic = await _unitOfWork.Repository<SiteAnalytic>().GetById(id);
            if (analytic == null)
                throw new Exception($"Site analytic with ID {id} not found");
            
            return _mapper.Map<SiteAnalyticResponse>(analytic);
        }

        public async Task<SiteAnalyticResponse> CreateAsync(SiteAnalyticRequest request)
        {
            var analytic = new SiteAnalytic
            {
                SiteName = request.SiteName,
                Count = request.Count,
                Note = request.Note,
                CreatedDate = DateTime.UtcNow.AddHours(7)
            };

            await _unitOfWork.Repository<SiteAnalytic>().InsertAsync(analytic);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SiteAnalyticResponse>(analytic);
        }

        public async Task<SiteAnalyticResponse> UpdateAsync(int id, SiteAnalyticRequest request)
        {
            var analytic = await _unitOfWork.Repository<SiteAnalytic>().GetById(id);
            if (analytic == null)
                throw new Exception($"Site analytic with ID {id} not found");

            analytic.SiteName = request.SiteName;
            analytic.Count = request.Count ?? analytic.Count.GetValueOrDefault();
            analytic.Note = request.Note;
            analytic.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _unitOfWork.Repository<SiteAnalytic>().Update(analytic, id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SiteAnalyticResponse>(analytic);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var analytic = await _unitOfWork.Repository<SiteAnalytic>().GetById(id);
            if (analytic == null)
                return false;

            analytic.IsDelete = true;
            analytic.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _unitOfWork.Repository<SiteAnalytic>().Update(analytic, id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<SiteAnalyticResponse> IncrementCountAsync(int id)
        {
            var analytic = await _unitOfWork.Repository<SiteAnalytic>().GetById(id);
            if (analytic == null)
                throw new Exception($"Site analytic with ID {id} not found");

            analytic.Count = (analytic.Count.GetValueOrDefault() + 1);
            analytic.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _unitOfWork.Repository<SiteAnalytic>().Update(analytic, id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SiteAnalyticResponse>(analytic);
        }
    }
} 