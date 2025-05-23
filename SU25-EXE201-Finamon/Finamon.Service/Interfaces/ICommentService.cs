using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface ICommentService
    {
        Task<PaginatedResponse<CommentResponse>> GetAllCommentsAsync(CommentQueryRequest queryRequest);
        Task<CommentResponse> GetCommentByIdAsync(int id);
        Task<PaginatedResponse<CommentResponse>> GetCommentsByPostIdAsync(int postId, CommentQueryRequest queryRequest);
        Task<PaginatedResponse<CommentResponse>> GetCommentsByUserIdAsync(int userId, CommentQueryRequest queryRequest);
        Task<CommentResponse> CreateCommentAsync(CommentRequest commentRequest);
        Task<CommentResponse> UpdateCommentAsync(int id, CommentUpdateRequest commentUpdateRequest, int userId); // userId to verify ownership for update
        Task<bool> DeleteCommentAsync(int id, int userId); // userId to verify ownership for delete
    }
} 