using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface ICategoryAlertService
    {
        Task<PaginatedResponse<CategoryAlertResponse>> GetAllCategoryAlertsAsync(CategoryAlertQueryRequest queryRequest);
        Task<CategoryAlertResponse> GetCategoryAlertByIdAsync(int id);
        Task<CategoryAlertResponse> CreateCategoryAlertAsync(CategoryAlertRequestModel request);
        Task<CategoryAlertResponse> UpdateCategoryAlertAsync(int id, CategoryAlertRequestModel request);
        Task DeleteCategoryAlertAsync(int id);
        Task<PaginatedResponse<CategoryAlertResponse>> GetAlertsByBudgetCategoryIdAsync(int budgetCategoryId, CategoryAlertQueryRequest queryRequest);
    }
} 