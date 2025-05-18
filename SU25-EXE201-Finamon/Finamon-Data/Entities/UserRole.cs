using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }

        public UserRole()
        {
            CreatedDate = DateTime.UtcNow.AddHours(7); // Convert UTC to UTC + 7
        }
    }
}
