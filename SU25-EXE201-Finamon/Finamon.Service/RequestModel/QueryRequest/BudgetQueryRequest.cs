using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BudgetQueryRequest : BaseQuery
    {
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 