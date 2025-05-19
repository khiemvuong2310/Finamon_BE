using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CategoryRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }
} 