using Microsoft.AspNetCore.Components;
using Localist.Shared;

namespace Localist.Client.Components
{
    public class PostReplyCardBase : ComponentBase
    {
        [Parameter]
        public TreeItem<PostReply> PostReplyTree { get; set; } = default!;

        public PostReply PostReply => PostReplyTree.Item;

        public string CardId => $"reply-{PostReply.ShortId}";

        public bool ShowReplyForm { get; set; }

        public void ToggleReplyForm(bool? show = null)
        {
            ShowReplyForm = show ?? !ShowReplyForm;
        }

        public void AppendReply(PostReply reply)
        {
            var replyTreeItem = new TreeItem<PostReply>(reply, new());
            PostReplyTree.Children.Add(replyTreeItem);
            ToggleReplyForm(false);
        }
    }
}
