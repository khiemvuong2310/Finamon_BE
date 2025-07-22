using System;
using Finamon_Data.Enum;

namespace Finamon.Service.ReponseModel
{
    public class ReceiptResponseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public decimal? Amount { get; set; }
        public ReceiptStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 