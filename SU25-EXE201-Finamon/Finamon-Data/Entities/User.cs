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
        public string? UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? Token { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public bool EmailVerified { get; set; } = false;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;

        //Navigation properties
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<ChatSession> ChatSessions { get; set; }
        public virtual ICollection<UserMembership> UserMemberships { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<UserActivity> UserActivities { get; set; }
    }
}
