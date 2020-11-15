using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Localist.Shared;

namespace Localist.Client.Components
{
    public class BrowserNotificationsButtondBase : ComponentBase
    {
        [Inject]
        public IJSRuntime JSRuntime { get; init; } = default!;

        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Parameter]
        public bool? HideIfSubscribed { get; init; }

        public bool IsSubscribed { get; set; }

        public bool HideComponent { get; set; } = true;

        public string? BrowserNotificationMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var subscriptionResult = await JSRuntime.InvokeAsync<BrowserSubscriptionResult>(
                "blazorPushNotifications.getSubscription")
                ?? throw new System.Exception("Failed get browser push subscription");

            if (subscriptionResult.Message is not null)
            {
                BrowserNotificationMessage = subscriptionResult.Message;
            }
            else
            {
                IsSubscribed = subscriptionResult.HasExistingSubscription == true;
            }

            HideComponent = HideIfSubscribed == true && IsSubscribed;
        }

        public void OnEnableNotificationsChange(ChangeEventArgs e)
        {
            IsSubscribed = (bool)e.Value!;

            if (IsSubscribed) _ = RequestNotificationSubscriptionAsync();
        }

        async Task RequestNotificationSubscriptionAsync()
        {
            try
            {
                var subscriptionResult = await JSRuntime.InvokeAsync<BrowserSubscriptionResult>(
                    "blazorPushNotifications.requestSubscription")
                    ?? throw new System.Exception("Failed to request browser push subscription");

                if (subscriptionResult.Message is not null)
                {
                    BrowserNotificationMessage = subscriptionResult.Message;
                    IsSubscribed = false;
                    StateHasChanged();
                }
                else if (subscriptionResult.NewSubscription is not null)
                {
                    BrowserNotificationMessage = null;
                    StateHasChanged();

                    var response = await Http.PutAsJsonAsync("api/Profile/notification-subscription", subscriptionResult.NewSubscription);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (System.Exception ex)
            {
                BrowserNotificationMessage = ex.Message;
                IsSubscribed = false;
                StateHasChanged();
            }
        }
    }

    record BrowserSubscriptionResult(
        NotificationSubscription? NewSubscription,
        string? Message,
        bool? HasExistingSubscription);
}