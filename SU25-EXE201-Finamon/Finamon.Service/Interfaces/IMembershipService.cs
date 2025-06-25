using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IMembershipService
    {
        Task<PaginatedResponse<MembershipResponse>> GetAllMembershipsAsync(MembershipQueryRequest queryRequest);
        Task<MembershipResponse> GetMembershipByIdAsync(int id);
        Task<MembershipResponse> CreateMembershipAsync(CreateMembershipRequest request);
        Task<MembershipResponse> UpdateMembershipAsync(int id, UpdateMembershipRequest request);
        Task<bool> DeleteMembershipAsync(int id);
        Task<bool> AssignMembershipToUserAsync(AssignMembershipRequest request);
        Task<PaginatedResponse<UserMembershipResponse>> GetAllUserMembershipsAsync(UserMembershipQueryRequest queryRequest);
    }
} 