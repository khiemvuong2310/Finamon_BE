using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CategoryRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string? Color { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
    }
    public class CreateManyCategoriesRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one category is required")]
        [MaxLength(50, ErrorMessage = "Cannot create more than 50 categories at once")]
        public List<CategoryRequestModel> Categories { get; set; }
    }
} 