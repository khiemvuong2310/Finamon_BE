using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class CommentRequest
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 1000 characters")]
        public string Content { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "PostId (BlogId) is required")]
        public int PostId { get; set; }
    }

    public class CommentUpdateRequest
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 1000 characters")]
        public string Content { get; set; }
    }
} 