using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class ReportQueryRequest : BaseQuery
    {
        public string? Title { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 