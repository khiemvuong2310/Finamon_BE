using System;
using System.ComponentModel.DataAnnotations;
using Finamon_Data.Enum;

namespace Finamon.Service.RequestModel
{
    public class ReceiptRequestModel
    {

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "MembershipId is required")]
        public int MembershipId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        public ReceiptStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class ReceiptUpdateRequestModel
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "MembershipId is required")]
        public int MembershipId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        public ReceiptStatus? Status { get; set; }
        public string? Note { get; set; }
    }
} 