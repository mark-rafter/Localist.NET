using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Threading.Tasks;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface INotificationService
    {
        Task Subscribe(NotificationSubscription subscription, string userId);
    }

    public class NotificationService : INotificationService
    {
        readonly IDbContext dbContext;
        readonly ILogger<NotificationService> logger;

        public NotificationService(
            IDbContext dbContext,
            ILogger<NotificationService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Subscribe(NotificationSubscription subscription, string userId)
        {
            var removeExistingSubscription = Builders<Profile>.Update
                .PullFilter(p => p.NotificationSubscriptions, ns => ns.UserAgent == subscription.UserAgent);

            await dbContext.Profiles.FindOneAndUpdateAsync(ud => ud.UserId == userId, removeExistingSubscription);

            var insertNewSubscription = Builders<Profile>.Update
                .Push(p => p.NotificationSubscriptions, subscription);

            await dbContext.Profiles.FindOneAndUpdateAsync(ud => ud.UserId == userId, insertNewSubscription);
        }
    }
}
