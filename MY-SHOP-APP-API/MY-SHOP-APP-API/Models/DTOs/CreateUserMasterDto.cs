using System;
using System.ComponentModel.DataAnnotations;

namespace MY_SHOP_APP_API.Models.DTOs
{
    public class CreateUserMasterDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Address1 { get; set; }

        [MaxLength(100)]
        public string? Address2 { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
