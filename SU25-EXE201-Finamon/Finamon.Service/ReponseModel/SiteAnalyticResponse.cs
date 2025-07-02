namespace Finamon.Service.ReponseModel
{
    public class SiteAnalyticResponse
    {
        public int Id { get; set; }
        public string? SiteName { get; set; }
        public int? Count { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 