using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Budget : BaseEntity
    {
        public string? Name { get; set; } 
        public string? Description { get; set; } 
        public decimal? Limit { get; set; } 
        public decimal? CurrentAmount { get; set; } 
        public int UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true; // Trạng thái active của budget

        // Navigation properties
        public ICollection<Expense> Expenses { get; set; }
        public ICollection<BudgetAlert> Alerts { get; set; }
        public ICollection<BudgetDetail> BudgetDetails { get; set; }
        public User User { get; set; }
    }
}
