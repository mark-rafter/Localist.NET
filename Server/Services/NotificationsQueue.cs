using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Localist.Server.Services
{
    public interface INotificationsQueue
    {
        void Enqueue(NotificationQueueItem queueItem);
        Task<NotificationQueueItem?> DequeueAsync(CancellationToken cancellationToken);
    }

    public class NotificationsQueue : INotificationsQueue
    {
        readonly ConcurrentQueue<NotificationQueueItem> queueItems = new();
        readonly SemaphoreSlim signal = new(0);

        public void Enqueue(NotificationQueueItem queueItem)
        {
            queueItems.Enqueue(queueItem);
            signal.Release();
        }

        public async Task<NotificationQueueItem?> DequeueAsync(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            queueItems.TryDequeue(out NotificationQueueItem? queueItem);
            return queueItem;
        }
    }

    public record NotificationQueueItem(
        /// <remarks>the user who generated the notification event</remarks>
        string UserId,
        /// <remarks>the entity whose watchers should be notified</remarks>
        string EntityId); // todo?: params string[] EntityIds);
}