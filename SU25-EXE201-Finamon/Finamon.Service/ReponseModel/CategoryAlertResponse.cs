namespace Finamon.Service.ReponseModel
{
    public class CategoryAlertResponse
    {
        public int Id { get; set; }
        public int BudgetCategoryId { get; set; }
        public float AlertThreshold { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 