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
        public decimal MonthlyPrice { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal YearlyPrice { get; set; }
    }

    public class UpdateMembershipRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal? MonthlyPrice { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal? YearlyPrice { get; set; }
    }

    public class MembershipQueryRequest : QueryRequest.BaseQuery
    {
        public string? Name { get; set; }
        public decimal? MinMonthlyPrice { get; set; }
        public decimal? MaxMonthlyPrice { get; set; }
        public decimal? MinYearlyPrice { get; set; }
        public decimal? MaxYearlyPrice { get; set; }
    }
} 