using System.ComponentModel.DataAnnotations;

namespace Localist.Shared
{
    public class LostCodeModel
    {
        [Required]
        [MaxLength(100, ErrorMessage = "{0} must be at most {1} characters.")]
        public string Address { get; set; } = default!;
    }
}
