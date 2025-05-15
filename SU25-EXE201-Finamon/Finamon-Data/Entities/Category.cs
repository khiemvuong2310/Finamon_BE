using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<Expense> Expenses { get; set; }
    }
}
