using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class UserMembershipQueryRequest : BaseQuery
    {
        public string? MembershipType { get; set; } // "Free" or "VIP"
        public bool? IsActive { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
} 