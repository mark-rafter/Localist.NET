using System.ComponentModel.DataAnnotations;

namespace Localist.Shared
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; }
    }
}
