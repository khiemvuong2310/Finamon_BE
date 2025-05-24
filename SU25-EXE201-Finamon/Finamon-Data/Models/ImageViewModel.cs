using System;

namespace Finamon_Data.Models
{
    public class ImageViewModel
    {
        public int Id { get; set; }
        public string Base64Image { get; set; }
        public string ContentType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 