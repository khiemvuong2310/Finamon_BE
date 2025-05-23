using System;

namespace Finamon.Service.ReponseModel
{
    public class ImageResponse
    {
        public int Id { get; set; }
        public string Base64Image { get; set; }
        public string ContentType { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 