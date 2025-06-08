using System;
using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class BudgetRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Limit is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than 0")]
        public decimal Limit { get; set; }

        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }
    }
}
