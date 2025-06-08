using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class BudgetCategoryRequestModel
    {
        [Required]
        public decimal MaxAmount { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public string? Description { get; set; }
    }
} 