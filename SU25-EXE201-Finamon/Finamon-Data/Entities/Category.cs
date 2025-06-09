using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finamon_Data.Entities
{
    public class Category : BaseEntity
    {
        public string? Name { get; set; }

        public string? Color { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Navigation properties
        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<BudgetCategory> BudgetCategorys { get; set; }
    }
}
