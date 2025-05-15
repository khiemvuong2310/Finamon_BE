using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Membership : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<UserMembership> UserMemberships { get; set; }
    }
}
