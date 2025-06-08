namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BudgetCategoryQueryRequest : BaseQuery
    {
        public int? CategoryId { get; set; }
        public decimal? MinMaxAmount { get; set; }
        public decimal? MaxMaxAmount { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 