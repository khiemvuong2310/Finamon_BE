using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class ChatSession : BaseEntity
    {
        public DateTime StartedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Chat> Chats { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
