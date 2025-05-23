using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBudgetDetailService
    {
        Task<PaginatedResponse<BudgetDetailResponse>> GetAllBudgetDetailsAsync(BudgetDetailQueryRequest queryRequest);
        Task<BudgetDetailResponse> GetBudgetDetailByIdAsync(int id);
        Task<BudgetDetailResponse> CreateBudgetDetailAsync(BudgetDetailRequestModel request);
        Task<BudgetDetailResponse> UpdateBudgetDetailAsync(int id, BudgetDetailRequestModel request);
        Task DeleteBudgetDetailAsync(int id);
        Task<PaginatedResponse<BudgetDetailResponse>> GetBudgetDetailsByBudgetIdAsync(int budgetId, BudgetDetailQueryRequest queryRequest);
        Task<PaginatedResponse<BudgetDetailResponse>> GetBudgetDetailsByCategoryIdAsync(int categoryId, BudgetDetailQueryRequest queryRequest);
    }
} 