using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class BudgetAlertRequestModel
    {
        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }

        [Required(ErrorMessage = "BudgetId is required")]
        public int BudgetId { get; set; }
    }
} 