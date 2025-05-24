using Microsoft.AspNetCore.Http;

namespace Finamon_Data.Models
{
    public class ImageCreateModel
    {
        public IFormFile File { get; set; }
    }
}