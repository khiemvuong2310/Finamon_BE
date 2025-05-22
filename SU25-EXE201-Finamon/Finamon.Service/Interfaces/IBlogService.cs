using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBlogService
    {
        Task<BaseResponse<BlogResponse>> CreateBlogAsync(CreateBlogRequest request);
        Task<BaseResponse<BlogResponse>> UpdateBlogAsync(int id, UpdateBlogRequest request);
        Task<BaseResponse> DeleteBlogAsync(int id);
        Task<BaseResponse<BlogResponse>> GetBlogByIdAsync(int id);
        Task<BaseResponse<PagedBlogResponse>> GetAllBlogsAsync(BlogFilterRequest filter);
    }
} 