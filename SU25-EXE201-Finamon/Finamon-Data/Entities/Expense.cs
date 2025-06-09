using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Expense :BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime? UpdateDate { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
