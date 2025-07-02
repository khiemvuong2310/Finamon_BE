using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;

namespace Finamon.Service.Interfaces
{
    public interface ISiteAnalyticService
    {
        Task<PaginatedResponse<SiteAnalyticResponse>> GetAllAsync(SiteAnalyticQueryRequest request);
        Task<SiteAnalyticResponse> GetByIdAsync(int id);
        Task<SiteAnalyticResponse> CreateAsync(SiteAnalyticRequest request);
        Task<SiteAnalyticResponse> UpdateAsync(int id, SiteAnalyticRequest request);
        Task<bool> DeleteAsync(int id);
        Task<SiteAnalyticResponse> IncrementCountAsync(int id);
    }
} 