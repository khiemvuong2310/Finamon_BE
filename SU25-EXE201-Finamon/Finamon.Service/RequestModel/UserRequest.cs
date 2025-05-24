using System.ComponentModel.DataAnnotations;

namespace Finamon.Service.RequestModel
{
    public class UserRequest
    {
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$", ErrorMessage = "Password must include letters and numbers.")]
        public string Password { get; set; }

        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Invalid Vietnamese phone number.")]
        public string? Phone { get; set; }

        public string? Location { get; set; }

        public string? Image { get; set; }

        [Range(1, 3, ErrorMessage = "RoleId must be between 1 and 3.")]
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