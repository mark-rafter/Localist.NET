namespace Localist.Shared
{
    public record NotificationSubscription(
        string? UserAgent,
        string Url,
        string P256dh,
        string Auth
    );
}
