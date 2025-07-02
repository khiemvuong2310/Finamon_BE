using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class SiteAnalyticQueryRequest : BaseQuery
    {
        public string? SiteName { get; set; }
        public int? MinCount { get; set; }
        public int? MaxCount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
} 