using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class BudgetAlert : BaseEntity
    {
        public string Message { get; set; }
        public int BudgetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public Budget Budget { get; set; }
    }
}
