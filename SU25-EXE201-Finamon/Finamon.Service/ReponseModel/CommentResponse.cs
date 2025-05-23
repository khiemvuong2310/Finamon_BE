using System;

namespace Finamon.Service.ReponseModel
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } // To display the commenter's name
        public int PostId { get; set; } // Assuming 'PostId' refers to BlogId
        public string BlogTitle { get; set; } // To display the blog title
        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
} 