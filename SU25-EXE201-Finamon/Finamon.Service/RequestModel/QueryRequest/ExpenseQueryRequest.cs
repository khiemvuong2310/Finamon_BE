using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class ExpenseQueryRequest : BaseQuery
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CategoryId { get; set; }
    }
} 