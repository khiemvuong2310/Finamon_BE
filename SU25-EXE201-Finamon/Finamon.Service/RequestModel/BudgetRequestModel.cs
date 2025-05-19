using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.RequestModel
{
    public class BudgetRequestModel
    {
        public decimal Limit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int? BudgetId { get; set; } // Nullable for update
    }
}
