using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CategoryAlertRequestModel
    {
        [Required(ErrorMessage = "BudgetCategoryId is required")]
        public int BudgetCategoryId { get; set; }

        [Required(ErrorMessage = "AlertThreshold is required")]
        [Range(0, 100, ErrorMessage = "AlertThreshold must be between 0 and 100")]
        public float AlertThreshold { get; set; }
    }
} 