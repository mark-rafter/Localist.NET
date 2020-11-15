using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Localist.Server.Clients;
using Localist.Server.Helpers;
using Localist.Shared;
using Localist.Shared.Helpers;

namespace Localist.Server.Services
{
    public interface IPostService
    {
        Task<PostDetail> AddPost(NewPostModel postModel, ClaimsPrincipal user);
        Task<PostReply> AddReply(NewPostReplyModel replyModel, ClaimsPrincipal user);
        Task ArchivePost(string id, string userId, bool archive);
        Task<Post?> GetPost(string id);
        Task<PostDetail?> GetPostDetail(string postId);
        Task<PostListResult<PostResult>> GetPostList(int page);
        Task<PostListResult<BookmarkedPostResult>> GetPostList(int page, HashSet<string> bookmarkedIds, HashSet<string> watchedIds);
        Task<IReadOnlyList<TreeItem<PostReply>>> GetReplyTrees(string postId);
    }

    public class PostService : IPostService
    {
        readonly IDbContext dbContext;
        readonly IImageUploadApi imageUploadService;
        readonly INotificationsQueue notificationsQueue;
        readonly ILogger<PostService> logger;

        public PostService(
            IDbContext dbContext,
            IImageUploadApi imageUploadService,
            INotificationsQueue notificationsQueue,
            ILogger<PostService> logger)
        {
            this.dbContext = dbContext;
            this.imageUploadService = imageUploadService;
            this.notificationsQueue = notificationsQueue;
            this.logger = logger;
        }

        public async Task<PostDetail> AddPost(NewPostModel postModel, ClaimsPrincipal user)
        {
            var imageUploadTasks = Task.WhenAll(postModel.Base64Images.Select(i => imageUploadService.Upload(i)));

            var postAuthor = !postModel.IsAnonymous
                ? new PostAuthor(user.GetUserId(), user.GetUserName())
                : null;

            var exchangeDetails = postModel.Type == PostType.Exchange
                ? new ExchangeDetails(postModel.Price ?? 0, postModel.ExchangeType, postModel.Unit)
                : null;

            var imageUploads = await imageUploadTasks;

            var post = new Post(
                    postModel.Title,
                    postAuthor,
                    postModel.Type,
                    exchangeDetails,
                    imageUploads.Any() ? imageUploads.First().Thumb.Url : null);

            post = await dbContext.Posts.Upsert(post);

            var postDetail = new PostDetail(
                post.Id ?? throw new KeyNotFoundException("post.Id was null"),
                postModel.Description,
                imageUploads
            );

            return await dbContext.PostDetails.Upsert(postDetail);
        }

        public async Task<PostReply> AddReply(NewPostReplyModel replyModel, ClaimsPrincipal user)
        {
            var postAuthor = !replyModel.IsAnonymous
                    ? new PostAuthor(user.GetUserId(), user.GetUserName())
                    : null;

            var postReply = new PostReply(
                replyModel.PostId,
                replyModel.PostReplyId,
                replyModel.Message,
                postAuthor);

            var result = await dbContext.PostReplies.Upsert(postReply);
            notificationsQueue.Enqueue(new(user.GetUserId(), replyModel.PostId));
            return result;
        }

        public async Task ArchivePost(string id, string userId, bool archive)
        {
            var post = await dbContext.Posts.SingleOrDefaultAsync(id)
                ?? throw new InvalidOperationException($"Post with id {id} does not exist");

            if (post.Author is null)
                throw new InvalidOperationException("You do not have permission to archive anonymous posts");

            if (post.Author.UserId != userId)
                throw new InvalidOperationException("You do not have permission to archive this post");

            await dbContext.Posts.Upsert(post with { IsArchived = archive });
        }

        public Task<Post?> GetPost(string id)
            => dbContext.Posts.SingleOrDefaultAsync(id);

        public async Task<PostDetail?> GetPostDetail(string postId)
            => (await dbContext.PostDetails.FindAsync(p => p.PostId == postId)).SingleOrDefault();

        public async Task<PostListResult<PostResult>> GetPostList(int page)
        {
            var filterDefinition = Builders<Post>.Filter.Where(p => !p.IsArchived);
            var sortDefinition = Builders<Post>.Sort.Descending(x => x.CreatedOn);

            var (totalPages, postList) = 
                await dbContext.Posts.AggregateByPage(page, 30, filterDefinition, sortDefinition);

            var getPostResultTasks = postList
                .Select(p => GetPostResult(p))
                .ToList();

            return new PostListResult<PostResult>(totalPages, await Task.WhenAll(getPostResultTasks));
        }

        // todo: unit test and refactor
        public async Task<PostListResult<BookmarkedPostResult>> GetPostList(int page, HashSet<string> bookmarkedIds, HashSet<string> watchedIds)
        {
            var allIds = bookmarkedIds.Union(watchedIds);
            var filterDefinition = Builders<Post>.Filter.Where(p => allIds.Contains(p.Id!));

            var (totalPages, postList) = await dbContext.Posts.AggregateByPage(page, 30, filterDefinition);

            var getPostResultTasks = postList
                .Select(p => GetPostResult(
                    p,
                    isBookmarked: bookmarkedIds.Contains(p.Id!),
                    isWatched: watchedIds.Contains(p.Id!)))
                .ToList();

            return new PostListResult<BookmarkedPostResult>(totalPages, await Task.WhenAll(getPostResultTasks));
        }

        public async Task<IReadOnlyList<TreeItem<PostReply>>> GetReplyTrees(string postId)
        {
            // build dictionary with empty children
            var postRepliesDict = (await dbContext.PostReplies.Find(r => r.PostId == postId).ToListAsync())
                .Select(reply => new
                {
                    reply.ParentId,
                    ReplyTree = new TreeItem<PostReply>(reply, new())
                })
                .ToDictionary(x => x.ReplyTree.Item.Id!);

            var result = new List<TreeItem<PostReply>>();

            foreach (var postReplyDict in postRepliesDict.Values)
            {
                if (postReplyDict.ParentId is not null)
                {
                    // add ReplyTree to parent
                    postRepliesDict[postReplyDict.ParentId]
                        .ReplyTree
                        .Children
                        .Add(postReplyDict.ReplyTree);
                }
                else
                {
                    // add top-level ReplyTree to result
                    result.Add(postReplyDict.ReplyTree);
                }
            }

            return result;
        }

        async Task<PostResult> GetPostResult(Post post)
        {
            var replyCount = await dbContext.PostReplies.CountDocumentsAsync(r => r.PostId == post.Id);
            return new PostResult(post, replyCount);
        }

        async Task<BookmarkedPostResult> GetPostResult(Post post, bool isBookmarked, bool isWatched)
        {
            var replyCount = await dbContext.PostReplies.CountDocumentsAsync(r => r.PostId == post.Id);
            return new BookmarkedPostResult(post, replyCount, isBookmarked, isWatched);
        }
    }
}
