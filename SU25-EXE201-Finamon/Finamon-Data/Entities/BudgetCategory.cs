using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class BudgetCategory: BaseEntity
    {
        public decimal MaxAmount { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<CategoryAlert> CategoryAlerts { get; set; }
    }
}
