namespace Finamon.Service.RequestModel.QueryRequest
{
    public class CategoryAlertQueryRequest : BaseQuery
    {
        public int? BudgetCategoryId { get; set; }
        public float? MinThreshold { get; set; }
        public float? MaxThreshold { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 