using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finamon.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(UserRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> GetUserByEmailAsync(string email);
        Task<UserResponse> GetUserByIdAsync(int id);
        Task<UserResponse> GetUserByUsernameAsync(string username);
        Task<UserDetailResponse> GetUserDetailByIdAsync(int id);
        Task<PaginatedResponse<UserResponse>> GetUsersByFilterAsync(UserQueryRequest query);
        Task<UserResponse> UpdateUserAsync(int id, UserUpdateRequest request);
        Task<UserResponse> UpdateUserImageAsync(int id, IFormFile imageFile);
        Task<int> GetTotalActiveUsersAsync();
    }
}