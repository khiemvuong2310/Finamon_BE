using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;

namespace Finamon.Service.Interfaces
{
    public interface IKeywordService
    {
        Task<KeywordResponse> CreateKeywordAsync(CreateKeywordRequest request);
        Task<KeywordResponse> UpdateKeywordAsync(Guid id, UpdateKeywordRequest request);
        Task<KeywordResponse> GetKeywordByIdAsync(Guid id);
        Task<PaginatedResponse<KeywordResponse>> GetKeywordsAsync(KeywordQueryRequest request);
        Task DeleteKeywordAsync(Guid id);
    }
} 