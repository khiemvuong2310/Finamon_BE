using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class CategoryAlert : BaseEntity
    {
        public int BudgetCategoryId { get; set; }
        public float AlertThreshold { get; set; }  // Percentage threshold for alert
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        // Navigation property
        public BudgetCategory BudgetCategory { get; set; }
    }
} 