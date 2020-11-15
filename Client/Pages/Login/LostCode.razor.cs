using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Localist.Shared;

namespace Localist.Client.Pages
{
    public class LostCodeBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Inject]
        public IToastService ToastService { get; init; } = default!;

        public LostCodeModel LostCodeModel = new();

        public bool IsSubmitting { get; set; }

        public string? Error { get; set; }

        public async Task HandleValidSubmitAsync(EditContext editContext)
        {
            IsSubmitting = true;

            if (!editContext.Validate())
            {
                Error = "Invalid address.";
                IsSubmitting = false;
                return;
            }

            var response = await Http.PostAsJsonAsync("api/Account/lost-code", LostCodeModel);

            if (response.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess("Your request has been submitted. A code will be posted to you shortly.");
            }
            else
            {
                Error = response.ReasonPhrase;
                IsSubmitting = false;
            }
        }
    }
}