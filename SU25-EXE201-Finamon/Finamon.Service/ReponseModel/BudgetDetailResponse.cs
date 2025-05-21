namespace Finamon.Service.ReponseModel
{
    public class BudgetDetailResponse
    {
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public string? BudgetName { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 