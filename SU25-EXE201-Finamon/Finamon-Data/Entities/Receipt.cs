using Finamon_Data.Enum;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finamon_Data.Entities
{
    public class Receipt : BaseEntity
    {
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public decimal? Amount { get; set; }
        public ReceiptStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public User User { get; set; }
        public Membership Membership { get; set; }
    }
} 