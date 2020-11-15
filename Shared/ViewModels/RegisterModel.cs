using System.ComponentModel.DataAnnotations;

namespace Localist.Shared
{
    public class RegisterModel
    {
        [Required]
        [MinLength(2, ErrorMessage = "{0} must be at least {1} characters.")]
        [MaxLength(30, ErrorMessage = "{0} must be at most {1} characters.")]
        public string Username { get; set; } = default!;

        [Required]
        [MinLength(8, ErrorMessage = "{0} must be at least {1} characters.")]
        [MaxLength(100, ErrorMessage = "{0} must be at most {1} characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        [MinLength(6, ErrorMessage = "{0} is too short.")]
        [MaxLength(100, ErrorMessage = "{0} is too long.")]
        public string InviteCode { get; set; } = default!;
    }
}
