using System.ComponentModel.DataAnnotations;

namespace MY_SHOP_APP_API.Models.DTOs
{
    public class LoginRequestDto
    {
        // Allow login with either email or phone
        public string? Email { get; set; }
        public string? Phone { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
