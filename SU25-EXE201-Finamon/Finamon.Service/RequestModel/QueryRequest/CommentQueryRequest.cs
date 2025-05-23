using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class CommentQueryRequest : BaseQuery
    {
        public int? UserId { get; set; }
        public int? PostId { get; set; }
        public string? ContentSearch { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
} 