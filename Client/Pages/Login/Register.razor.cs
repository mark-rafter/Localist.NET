using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Localist.Shared;

namespace Localist.Client.Pages
{
    public class RegisterBase : ComponentBase
    {
        [Inject]
        public IAuthService AuthService { get; init; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; init; } = default!;

        public RegisterModel RegisterModel = new();

        public bool IsSubmitting { get; set; }

        public string? Error { get; set; }

        public async Task HandleValidSubmitAsync(EditContext editContext)
        {
            IsSubmitting = true;

            if (!editContext.Validate())
            {
                IsSubmitting = false;
                return;
            }

            var result = await AuthService.Register(RegisterModel);

            if (result.Successful)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                Error = result.Error;
                IsSubmitting = false;
            }
        }
    }
}