namespace Finamon.Service.ReponseModel
{
    public class BudgetResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Limit { get; set; }
        public decimal? CurrentAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public ICollection<BudgetDetailResponse> BudgetDetails { get; set; }
        public ICollection<BudgetAlertResponse> Alerts { get; set; }
    }
} 