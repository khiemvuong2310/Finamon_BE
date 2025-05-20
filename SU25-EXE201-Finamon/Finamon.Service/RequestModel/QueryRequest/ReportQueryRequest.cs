using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class ReportQueryRequest
    {
        public string Title { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 