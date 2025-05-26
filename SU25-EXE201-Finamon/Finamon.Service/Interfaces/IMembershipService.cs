using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
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
    }
} 