using System;

namespace Finamon.Service.ReponseModel
{
    public class BudgetAlertResponse
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreateAt { get; set; }

        public DateTime? UpdateDate { get; set; }
        public int BudgetId { get; set; }
        public BudgetResponse Budget { get; set; }
    }
} 