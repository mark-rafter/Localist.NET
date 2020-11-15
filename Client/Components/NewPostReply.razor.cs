using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Localist.Shared;

namespace Localist.Client.Components
{
    public class NewPostReplyBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Inject]
        public IJSInProcessRuntime JSInProcessRuntime { get; init; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; init; } = default!;

        [Parameter]
        public string PostId { get; init; } = default!;

        [Parameter]
        public string? PostReplyId { get; init; }

        [Parameter]
        public EventCallback<PostReply> OnSubmitCallback { get; set; }

        public NewPostReplyModel NewPostReplyModel = default!;

        public string SectionId => $"new-reply-{PostId}-{PostReplyId}";

        public string MessageInputId => $"{SectionId}-message";

        public bool IsSubmitting { get; set; }

        public string? Error { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                JSInProcessRuntime.Invoke<string>("blazorAnimations.focus", MessageInputId);
            }
        }

        protected override void OnParametersSet()
        {
            NewPostReplyModel = new NewPostReplyModel()
            {
                PostId = PostId,
                PostReplyId = PostReplyId
            };
        }

        public async Task HandleValidSubmitAsync(EditContext editContext)
        {
            IsSubmitting = true;

            if (!editContext.Validate())
            {
                IsSubmitting = false;
                return;
            }

            var response = await Http.PostAsJsonAsync($"api/Post/reply", NewPostReplyModel);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadFromJsonAsync<PostReply>()
                    ?? throw new System.InvalidOperationException("Could not deserialise reply from response");

                await OnSubmitCallback.InvokeAsync(reply);
            }
            else
            {
                Error = response.ReasonPhrase;
                IsSubmitting = false;
            }
        }

    }
}
