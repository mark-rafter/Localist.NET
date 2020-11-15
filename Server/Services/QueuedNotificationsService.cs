using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Localist.Server.Config;
using WebPush;

namespace Localist.Server.Services
{
    public class QueuedNotificationsService : BackgroundService
    {
        readonly INotificationsQueue notificationsQueue;
        readonly IServiceProvider serviceProvider;
        readonly VapidDetails vapidDetails;
        readonly ILogger<QueuedNotificationsService> logger;

        public QueuedNotificationsService(
            INotificationsQueue notificationsQueue,
            IServiceProvider serviceProvider,
            IVapidOptions vapidOptions,
            ILogger<QueuedNotificationsService> logger)
        {
            this.notificationsQueue = notificationsQueue;
            this.serviceProvider = serviceProvider;
            this.vapidDetails = new VapidDetails(
                vapidOptions.Subject, vapidOptions.PublicKey, vapidOptions.PrivateKey);
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueuedNotificationsService is running");
            await BackgroundProcessing(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueuedNotificationsService is stopping");
            await base.StopAsync(cancellationToken);
        }

        async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var queueItem = await notificationsQueue.DequeueAsync(cancellationToken);

                try
                {
                    if (queueItem is not null && !cancellationToken.IsCancellationRequested)
                    {
                        await ProcessQueueItemAsync(queueItem);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred processing {queueItem}.", queueItem);
                }
            }
        }

        async Task ProcessQueueItemAsync(NotificationQueueItem queueItem)
        {
            var serviceScope = serviceProvider.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<IDbContext>();

            var watchersPushSubscriptions = dbContext.Profiles.AsQueryable()
                .Where(p => p.WatchIds.Contains(queueItem.EntityId))
                .Where(p => p.UserId != queueItem.UserId) // don't notify the replier of their own reply
                .SelectMany(p => p.NotificationSubscriptions)
                .Select(ns => new PushSubscription(ns.Url, ns.P256dh, ns.Auth))
                .ToList();

            if (watchersPushSubscriptions.Count > 0)
            {
                logger.LogInformation("Notifying {Count} watchers about {EntityId}",
                    watchersPushSubscriptions.Count, queueItem.EntityId);

                await SendNotificationsAsync(
                    watchersPushSubscriptions,
                    "Someone replied to a post you are watching",
                    $"post/{queueItem.EntityId}");
            }
        }

        async Task SendNotificationsAsync(List<PushSubscription> pushSubscriptions, string message, string url)
        {
            try
            {
                var webPushClient = new WebPushClient();
                var payload = JsonSerializer.Serialize(new { message, url });

                // todo: unit test exception handling
                var notificationTasks = pushSubscriptions.Select(s =>
                    webPushClient.SendNotificationAsync(s, payload, vapidDetails));

                await Task.WhenAll(notificationTasks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending push notification");
            }
        }
    }
}