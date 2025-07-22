using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;

namespace Finamon.Service.Interfaces
{
    public interface IReceiptService
    {
        Task<ReceiptResponseModel> CreateAsync(ReceiptRequestModel model);
        Task<bool> DeleteAsync(int id);
        Task<PaginatedResponse<ReceiptResponseModel>> GetAllAsync(ReceiptQueryRequest query);
        Task<ReceiptResponseModel> GetByIdAsync(int id);
        Task<ReceiptResponseModel> UpdateAsync(int Id ,ReceiptUpdateRequestModel model);
    }
}