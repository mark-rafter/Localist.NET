using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Localist.Server.Config;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface IDbContext
    {
        IMongoCollection<Invite> Invites { get; init; }
        IMongoCollection<LostCode> LostCodes { get; init; }
        IMongoCollection<Post> Posts { get; init; }
        IMongoCollection<PostDetail> PostDetails { get; init; }
        IMongoCollection<PostReply> PostReplies { get; init; }
        IMongoCollection<Profile> Profiles { get; init; }
    }

    public class MongoDbContext : IDbContext
    {
        readonly IMongoDatabase database;

        public IMongoCollection<Invite> Invites { get; init; }
        public IMongoCollection<LostCode> LostCodes { get; init; }
        public IMongoCollection<Post> Posts { get; init; }
        public IMongoCollection<PostDetail> PostDetails { get; init; }
        public IMongoCollection<PostReply> PostReplies { get; init; }
        public IMongoCollection<Profile> Profiles { get; init; }

        public MongoDbContext(IDatabaseOptions dbOptions)
        {
            var client = new MongoClient(dbOptions.ConnectionString);
            database = client.GetDatabase(dbOptions.Name);

            Invites = database.GetCollection<Invite>(nameof(Invite));
            LostCodes = database.GetCollection<LostCode>(nameof(LostCode));
            Posts = database.GetCollection<Post>(nameof(Post));
            PostReplies = database.GetCollection<PostReply>(nameof(PostReply));
            PostDetails = database.GetCollection<PostDetail>(nameof(PostDetail));
            Profiles = database.GetCollection<Profile>(nameof(Profile));

            // todos: create capped collection of Posts (200?) https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/admin/#creating-a-collection
            // : change streams for notifications? https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/change_streams/

            // todo: move to IHostedService https://kevsoft.net/2020/03/06/creating-mongodb-indexes-in-asp-net-core-3-1.html
            CreateIndexes();
        }

        public static void RegisterEntityMaps()
        {
            // set all DateTimeOffsets to store as string
            BsonSerializer.RegisterSerializer(
                typeof(DateTimeOffset),
                new DateTimeOffsetSerializer(BsonType.String));

            BsonSerializer.RegisterSerializer(
                typeof(DateTimeOffset?),
                new NullableSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String)));

            TryRegisterClassMap<DbEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIgnoreIfDefault(true);
            });

            TryRegisterClassMap<PostAuthor>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(a => a.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });

            TryRegisterClassMap<PostDetail>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(pd => pd.PostId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });

            TryRegisterClassMap<PostReply>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(pr => pr.PostId).SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(pr => pr.ParentId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });

            TryRegisterClassMap<Profile>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
                // todo?: convert strings to objectids
                // cm.MapMember(p => p.BookmarkIds).SetSerializer(new StringSerializer(BsonType.ObjectId));
                // cm.MapMember(p => p.WatchIds).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        void CreateIndexes()
        {
            Invites.Indexes.CreateOne(new CreateIndexModel<Invite>(
                Builders<Invite>.IndexKeys
                    .Ascending(x => x.Code)));

            Posts.Indexes.CreateOne(new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys
                    .Descending(x => x.CreatedOn),
                    new CreateIndexOptions<Post>
                    {
                        PartialFilterExpression = Builders<Post>.Filter.Eq(p => p.IsArchived, false)
                    }));

            PostReplies.Indexes.CreateOne(new CreateIndexModel<PostReply>(
                Builders<PostReply>.IndexKeys
                    .Descending(x => x.PostId)
                    .Descending(x => x.ParentId)));

            PostDetails.Indexes.CreateOne(new CreateIndexModel<PostDetail>(
                Builders<PostDetail>.IndexKeys
                    .Descending(x => x.PostId)));

            // commented out as notifications aren't time-critical
            // Profiles.Indexes.CreateOne(new CreateIndexModel<Profile>(
            //     Builders<Profile>.IndexKeys
            //         .Descending(x => x.WatchIds)));
        }

        static void TryRegisterClassMap<TClass>(Action<BsonClassMap<TClass>> classMapInitializer)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TClass)))
            {
                BsonClassMap.RegisterClassMap<TClass>(classMapInitializer);
            }
        }
    }
}
