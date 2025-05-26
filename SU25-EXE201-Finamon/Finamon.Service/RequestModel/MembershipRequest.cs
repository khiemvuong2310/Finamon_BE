using System;
using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CreateMembershipRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; } // Duration in days, for example
    }

    public class UpdateMembershipRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal? Price { get; set; }

        [Range(1, int.MaxValue)]
        public int? Duration { get; set; } // Duration in days
    }

    public class MembershipQueryRequest : QueryRequest.BaseQuery
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
    }
} 