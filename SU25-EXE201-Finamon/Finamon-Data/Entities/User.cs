using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class User :BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Token { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public int RoleId { get; set; }


        public Role Role { get; set; }

        public ICollection<Expense> Expenses { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<ChatSession> ChatSessions { get; set; }
        public ICollection<UserMembership> UserMemberships { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
