using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class SiteAnalyticRequest
    {
        [Required]
        public string SiteName { get; set; }

        public int? Count { get; set; }

        public string? Note { get; set; }
    }
} 