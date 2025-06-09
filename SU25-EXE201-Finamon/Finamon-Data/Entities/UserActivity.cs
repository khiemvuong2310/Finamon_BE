using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finamon_Data.Entities
{
    public class UserActivity : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Thời gian sử dụng (tính bằng phút)
        public int UseTime { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow.AddHours(7);
    }
} 