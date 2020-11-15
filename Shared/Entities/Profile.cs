using System.Collections.Generic;

namespace Localist.Shared
{
    public record Profile(
        string UserId,
        HashSet<string> BookmarkIds,
        HashSet<string> WatchIds,
        List<NotificationSubscription> NotificationSubscriptions
    ) : DbEntity;
}
