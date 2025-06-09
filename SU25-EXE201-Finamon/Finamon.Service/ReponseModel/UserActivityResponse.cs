using System;

namespace Finamon.Service.ReponseModel
{
    public class UserActivityResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UseTime { get; set; }  // Thời gian sử dụng (phút)
        public DateTime CreatedDate { get; set; }
    }

    public class UserActivityStatsResponse
    {
        public int TotalActivities { get; set; }  // Tổng số lần đăng nhập
        public int TotalMinutes { get; set; }     // Tổng số phút sử dụng
        public double AverageMinutes { get; set; } // Trung bình số phút mỗi lần
        public DateTime LastActivity { get; set; }  // Lần hoạt động gần nhất
    }
} 