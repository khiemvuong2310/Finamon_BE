namespace Finamon.Service.ReponseModel
{
    public class BudgetCategoryResponse
    {
        public int Id { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public ICollection<CategoryAlertResponse> CategoryAlerts { get; set; }
    }
} 