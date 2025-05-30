using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<PaginatedResponse<CategoryResponse>> GetAllCategoriesAsync(CategoryQueryRequest query);
        Task<CategoryResponse> GetCategoryByIdAsync(int id);
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequestModel request);
        Task<CategoryResponse> UpdateCategoryAsync(int id, CategoryRequestModel request);
        Task DeleteCategoryAsync(int id);
    }
} 