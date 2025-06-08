using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBudgetCategoryService
    {
        Task<PaginatedResponse<BudgetCategoryResponse>> GetAllBudgetCategoriesAsync(BudgetCategoryQueryRequest queryRequest);
        Task<BudgetCategoryResponse> GetBudgetCategoryByIdAsync(int id);
        Task<BudgetCategoryResponse> CreateBudgetCategoryAsync(BudgetCategoryRequestModel request);
        Task<BudgetCategoryResponse> UpdateBudgetCategoryAsync(int id, BudgetCategoryRequestModel request);
        Task DeleteBudgetCategoryAsync(int id);
        Task<PaginatedResponse<BudgetCategoryResponse>> GetBudgetCategoriesByCategoryIdAsync(int categoryId, BudgetCategoryQueryRequest queryRequest);
    }
} 