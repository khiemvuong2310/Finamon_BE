using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Chat :BaseEntity
    {
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ChatSessionId { get; set; }
        public ChatSession ChatSession { get; set; }
    }
}
