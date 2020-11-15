using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Localist.Shared;

namespace Localist.Client.Pages
{
    public class NewPostBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; init; } = default!;

        [Inject]
        public IImageUploadService ImageUploadService { get; init; } = default!;

        public NewPostModel NewPostModel = new();

        public EditContext EditContext = default!;

        public bool IsSubmitting { get; set; }

        public bool IsUploading { get; set; }

        public string? Error { get; set; }

        public System.Array PostTypes = System.Enum.GetValues(typeof(PostType));

        readonly CancellationTokenSource cts = new();

        const int maxAllowedFiles = 6;

        protected override void OnInitialized()
        {
            EditContext = new EditContext(NewPostModel);
        }

        public void Dispose()
        {
            cts.Cancel();
            // todo?: cts.Dispose();
        }

        public async Task HandleValidSubmitAsync(EditContext editContext)
        {
            IsSubmitting = true;

            if (!editContext.Validate())
            {
                IsSubmitting = false;
                return;
            }

            var response = await Http.PostAsJsonAsync("api/Post", NewPostModel);

            if (response.IsSuccessStatusCode)
            {
                var postId = await response.Content.ReadAsStringAsync();
                NavigationManager.NavigateTo($"/post/{postId}");
            }
            else
            {
                Error = response.ReasonPhrase;
                IsSubmitting = false;
            }
        }

        public async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            IsUploading = true;
            Error = null;
            try
            {
                EditContext.NotifyFieldChanged(FieldIdentifier.Create(() => NewPostModel.Base64Images));

                var files = e.GetMultipleFiles(maxAllowedFiles);

                await foreach (var base64File in ImageUploadService
                    .UploadFilesAsync(files)
                    .WithCancellation(cts.Token))
                {
                    NewPostModel.Base64Images.Add(base64File);
                }
                Error = ImageUploadService.Error;
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}