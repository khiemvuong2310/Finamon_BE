using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class ExpenseRequest
    {
        public string? Name { get; set; }
        
        //[Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        //[Required(ErrorMessage = "Date is required")]
        //public DateTime Date { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }
    }

    public class ExpenseUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        //public DateTime? Date { get; set; }
        public int? CategoryId { get; set; }
    }

    //public class ExpenseQueryRequest
    //{
    //    public string? Description { get; set; }
    //    public decimal? MinAmount { get; set; }
    //    public decimal? MaxAmount { get; set; }
    //    public DateTime? StartDate { get; set; }
    //    public DateTime? EndDate { get; set; }
    //    public int? CategoryId { get; set; }
    //    public int? BudgetId { get; set; }
    //    public int PageNumber { get; set; } = 1;
    //    public int PageSize { get; set; } = 10;
    //    public string? Sort { get; set; }
    //}
} 