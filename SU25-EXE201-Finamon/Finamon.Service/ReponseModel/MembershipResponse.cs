using System;

namespace Finamon.Service.ReponseModel
{
    public class MembershipResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int ActiveSubscriptions { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 