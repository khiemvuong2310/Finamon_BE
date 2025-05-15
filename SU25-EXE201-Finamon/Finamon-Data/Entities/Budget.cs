using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Budget : BaseEntity
    {
        public decimal Limit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<Expense> Expenses { get; set; }
        public ICollection<BudgetAlert> Alerts { get; set; }
    }
}
