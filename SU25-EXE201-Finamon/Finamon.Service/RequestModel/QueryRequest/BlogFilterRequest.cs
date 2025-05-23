using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BlogFilterRequest : BaseQuery
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserId { get; set; } // Optional: Filter by user who created the blog
        // Add any other specific filter properties for Blogs here
    }
} 