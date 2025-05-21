using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBudgetDetailService
    {
        Task<IEnumerable<BudgetDetailResponse>> GetAllBudgetDetailsAsync(BudgetDetailQueryRequest queryRequest = null);
        Task<BudgetDetailResponse> GetBudgetDetailByIdAsync(int id);
        Task<BudgetDetailResponse> CreateBudgetDetailAsync(BudgetDetailRequestModel request);
        Task<BudgetDetailResponse> UpdateBudgetDetailAsync(int id, BudgetDetailRequestModel request);
        Task DeleteBudgetDetailAsync(int id);
        Task<IEnumerable<BudgetDetailResponse>> GetBudgetDetailsByBudgetIdAsync(int budgetId);
        Task<IEnumerable<BudgetDetailResponse>> GetBudgetDetailsByCategoryIdAsync(int categoryId);
    }
} 