namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BudgetDetailQueryRequest : BaseQuery
    {
        public int? BudgetId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinMaxAmount { get; set; }
        public decimal? MaxMaxAmount { get; set; }
        public decimal? MinCurrentAmount { get; set; }
        public decimal? MaxCurrentAmount { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 