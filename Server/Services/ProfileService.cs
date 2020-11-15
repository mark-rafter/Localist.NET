using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Localist.Server.Helpers;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface IProfileService
    {
        Task AddToProfile(string userId, PatchProfileModel profileModel);
        Task<Profile> AddBookmark(string userId, string entityId, bool enableNotifications);
        Task<Profile> Create(string userId);
        Task<Profile> GetProfile(string userId);
        Task<bool> IsEntityBookmarked(string userId, string entityId);
        Task RemoveFromProfile(string userId, PatchProfileModel profileModel);
    }

    public class ProfileService : IProfileService
    {
        readonly IDbContext dbContext;
        readonly ILogger<ProfileService> logger;

        public ProfileService(
            IDbContext dbContext,
            ILogger<ProfileService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Profile> AddBookmark(string userId, string postId, bool enableNotifications)
        {
            var updateDefinition = Builders<Profile>.Update.Push(e => e.BookmarkIds, postId);

            if (enableNotifications)
            {
                updateDefinition = updateDefinition.Push(e => e.WatchIds, postId);
            }

            return await dbContext.Profiles.FindOneAndUpdateAsync(ud => ud.UserId == userId, updateDefinition);
        }

        public Task<Profile> Create(string userId)
        {
            var profile = new Profile(userId, new(), new(), new());
            return dbContext.Profiles.Upsert(profile);
        }

        public async Task<bool> IsEntityBookmarked(string userId, string entityId)
        {
            var profileWithBookmark = await dbContext.Profiles
                .Find(p => p.UserId == userId && p.BookmarkIds.Contains(entityId))
                .SingleOrDefaultAsync();

            return profileWithBookmark is not null;
        }

        public Task<Profile> GetProfile(string userId)
            => dbContext.Profiles.Find(p => p.UserId == userId).SingleAsync();

        public Task AddToProfile(string userId, PatchProfileModel profileModel)
            => PatchProfile(userId, profileModel);

        public Task RemoveFromProfile(string userId, PatchProfileModel profileModel)
            => PatchProfile(userId, profileModel, remove: true);

        async Task PatchProfile(string userId, PatchProfileModel profileModel, bool remove = false)
        {
            var update = Builders<Profile>.Update;
            var updates = new List<UpdateDefinition<Profile>>();

            // todo: reflection?
            if (remove)
            {
                if (profileModel.BookmarkIds?.Length > 0)
                    updates.Add(update.PullAll(e => e.BookmarkIds, profileModel.BookmarkIds));

                if (profileModel.WatchIds?.Length > 0)
                    updates.Add(update.PullAll(e => e.WatchIds, profileModel.WatchIds));
            }
            else
            {
                if (profileModel.BookmarkIds?.Length > 0)
                    updates.Add(update.AddToSetEach(e => e.BookmarkIds, profileModel.BookmarkIds));

                if (profileModel.WatchIds?.Length > 0)
                    updates.Add(update.AddToSetEach(e => e.WatchIds, profileModel.WatchIds));
            }

            await dbContext.Profiles.FindOneAndUpdateAsync(
                ud => ud.UserId == userId,
                update.Combine(updates));
        }
    }
}
