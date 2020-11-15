using Microsoft.AspNetCore.Components;
using Localist.Shared;

namespace Localist.Client.Components
{
    public class PostCardBase : ComponentBase
    {
        [Parameter]
        public Post Post { get; init; } = default!;

        [Parameter]
        public bool? IsBookmarked { get; set; }

        [Parameter]
        public bool? IsWatched { get; set; }

        [Parameter]
        public long ReplyCount { get; set; } = default!;

        public string GetLeftBorderStyle()
            => Post.Type switch
            {
                PostType.Exchange => (int)Post.ExchangeDetails!.Type % 2 == 0 ? "bl-info" : "bl-success",
                PostType.Message => "bl-secondary",
                _ => ""
            };

        public string GetPlaceholderSvg()
        {
            return Post.ExchangeDetails?.Type switch
            {
                ExchangeType.Buy => "svg/buy.svg",
                ExchangeType.Sell => "svg/price-tag.svg",
                null => "svg/note.svg",
                _ => "svg/handshake.svg",
            };
        }

        public string RenderReplyCount()
            => ReplyCount switch
            {
                1 => "1 reply",
                _ => $"{ReplyCount} replies"
            };
    }
}
