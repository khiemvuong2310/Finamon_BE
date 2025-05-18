namespace Finamon.Service.ReponseModel
{
    public class BudgetResponse
    {
        public int Id { get; set; }
        public decimal Limit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
} 