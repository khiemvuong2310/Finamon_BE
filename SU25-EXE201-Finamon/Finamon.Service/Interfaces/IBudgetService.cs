using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBudgetService
    {
        Task<PaginatedResponse<BudgetResponse>> GetAllBudgetsAsync(BudgetQueryRequest queryRequest);
        Task<BudgetResponse> GetBudgetByIdAsync(int id);
        Task<BudgetResponse> CreateBudgetAsync(BudgetRequestModel request);
        Task<BudgetResponse> UpdateBudgetAsync(int id, BudgetRequestModel request);
        Task DeleteBudgetAsync(int id);
    }
}