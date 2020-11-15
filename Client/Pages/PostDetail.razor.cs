using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Localist.Shared;
using Microsoft.Extensions.Logging;
using Blazored.Modal.Services;
using Blazored.Modal;
using Localist.Client.Components;

namespace Localist.Client.Pages
{
    public class PostDetailBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Inject]
        public ILogger<PostDetailBase> Logger { get; init; } = default!;

        [CascadingParameter]
        public IModalService Modal { get; init; } = default!;

        [Parameter]
        public string PostId { get; init; } = default!;

        public PostDetailResult? PostDetail { get; set; }

        public TreeItem<PostReply>[]? PostReplyTrees { get; set; }

        public string? Error { get; set; }

        public bool ShowReplyForm { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                PostDetail = await Http.GetFromJsonAsync<PostDetailResult>($"api/Post/detail/{PostId}");
                StateHasChanged();
                PostReplyTrees = await Http.GetFromJsonAsync<TreeItem<PostReply>[]>($"api/Post/replies/{PostId}");
            }
            catch (System.Exception ex)
            {
                Error = $"Failed to load post '{PostId}'";
                Logger.LogError(ex, Error);
                throw;
            }
        }

        public void ToggleIsArchived()
        {
            PostDetail = PostDetail! with { IsArchived = !PostDetail.IsArchived };
            _ = ToggleIsArchivedAsync();
        }

        async Task ToggleIsArchivedAsync()
        {
            // todo: refactor this and ToggleButton into single generic component w/ callback?
            var patchModel = new PatchPostModel(PostId, IsArchived: PostDetail!.IsArchived);

            var response = PostDetail!.IsArchived == true
                ? await Http.PutAsJsonAsync($"api/Post/add", patchModel)
                : await Http.PutAsJsonAsync($"api/Post/remove", patchModel);

            if (!response.IsSuccessStatusCode)
            {
                // revert back
                PostDetail = PostDetail! with { IsArchived = !PostDetail.IsArchived };
                // todo: trigger error toast
                StateHasChanged();
            }
        }

        public void ToggleReplyForm(bool? show = null)
        {
            ShowReplyForm = show ?? !ShowReplyForm;
        }

        public void RefreshReplies(PostReply reply)
        {
            ToggleReplyForm(false);

            PostReplyTrees = PostReplyTrees
                ?.Prepend(new TreeItem<PostReply>(reply, new()))
                ?.ToArray();
        }

        public void ShowImageModal(int index)
        {
            var parameters = new ModalParameters();
            parameters.Add("ImageUrls", PostDetail!.ImageUploadApiData!.Select(i => i.Url).ToArray());
            parameters.Add("Index", index);

            Modal.Show<ImageCarouselModal>("", parameters, new() { HideHeader = true });
        }
    }
}
