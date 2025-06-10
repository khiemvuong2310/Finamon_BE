using System;

namespace Finamon.Service.ReponseModel
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? Token { get; set; }

        public List<UserRoleResponseNameModel>? UserRoles { get; set; }

    }

    public class UserDetailResponse : UserResponse
    {
        public int TotalExpenses { get; set; }
        public decimal TotalExpenseAmount { get; set; }
        public int TotalReports { get; set; }
        public int TotalChatSessions { get; set; }
        public bool HasActiveMembership { get; set; }
    }
} 