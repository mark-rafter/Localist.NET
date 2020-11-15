using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Localist.Client.Components
{
    public class ToggleButtonBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Parameter]
        public bool? IsEnabled { get; set; }

        [Parameter]
        public string Icon { get; init; } = default!;

        [Parameter]
        public string? Title { get; init; }

        [Parameter]
        public string? Size { get; init; }

        [Parameter]
        public string Url { get; init; } = default!;

        [Parameter]
        public string Controller { get; init; } = default!;

        [Parameter]
        public object PatchModel { get; init; } = default!;

        bool isSubmitting;

        public void Toggle()
        {
            IsEnabled = !IsEnabled;
            _ = UpdateAsync();
        }

        async Task UpdateAsync()
        {
            if (isSubmitting) return;

            isSubmitting = true;
            try
            {
                var response = IsEnabled == true
                    ? await Http.PutAsJsonAsync($"api/{Controller}/add", PatchModel)
                    : await Http.PutAsJsonAsync($"api/{Controller}/remove", PatchModel);

                if (!response.IsSuccessStatusCode)
                {
                    IsEnabled = !IsEnabled;
                    // todo: trigger error toast
                    StateHasChanged();
                }
            }
            finally
            {
                isSubmitting = false;
            }
        }
    }
}