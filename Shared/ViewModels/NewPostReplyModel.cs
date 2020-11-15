using System.ComponentModel.DataAnnotations;

namespace Localist.Shared
{
    public class NewPostReplyModel
    {
        [Required]
        [StringLength(24, MinimumLength = 24)]
        public string PostId { get; init; } = default!;

        [StringLength(24, MinimumLength = 24)]
        public string? PostReplyId { get; init; }

        [Required]
        [MaxLength(2000, ErrorMessage = "{0} must be at most {1} characters.")]
        public string Message { get; set; } = default!;

        public bool IsAnonymous { get; set; }

        // public bool EnableNotifications { get; set; }
    }
}
