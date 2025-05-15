using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class UserMembership :BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int MembershipId { get; set; }
        public Membership Membership { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
