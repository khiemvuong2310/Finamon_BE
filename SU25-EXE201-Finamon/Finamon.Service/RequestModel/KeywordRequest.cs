using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CreateKeywordRequest
    {
        [Required(ErrorMessage = "Text is required")]
        [StringLength(255, ErrorMessage = "Text cannot exceed 255 characters")]
        public string Text { get; set; }
    }

    public class UpdateKeywordRequest
    {
        [Required(ErrorMessage = "Text is required")]
        [StringLength(255, ErrorMessage = "Text cannot exceed 255 characters")]
        public string Text { get; set; }
    }

}