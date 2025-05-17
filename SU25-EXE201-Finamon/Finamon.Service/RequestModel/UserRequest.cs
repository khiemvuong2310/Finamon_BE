using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class UserRequest
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Image { get; set; }
        public int RoleId { get; set; }
    }

    public class UserUpdateRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public int? RoleId { get; set; }
    }

    public class UserLoginRequest
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
} 