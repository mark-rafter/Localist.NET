using Microsoft.AspNetCore.Components;

namespace Localist.Client.Shared
{
    public class RedirectToLogin : ComponentBase
    {
        [Inject]
        protected NavigationManager NavigationManager { get; init; } = default!;

        protected override void OnInitialized()
        {
            NavigationManager.NavigateTo("/login");
        }
    }
}
