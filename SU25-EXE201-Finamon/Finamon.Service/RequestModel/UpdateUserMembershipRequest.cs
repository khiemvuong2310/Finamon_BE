using System;
using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class UpdateUserMembershipRequest
    {
        public int? MembershipId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsDelete { get; set; }
    }
} 