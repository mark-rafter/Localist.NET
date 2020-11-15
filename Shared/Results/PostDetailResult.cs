using System;
using System.Collections.Generic;

namespace Localist.Shared
{
    public record PostDetailResult : IPost, IPostDetail
    {
        public PostDetailResult() { }

        public PostDetailResult(IPost post, IPostDetail postDetail, DateTimeOffset postCreatedOn, bool isBookmarked)
        {
            // IPost
            Title = post.Title;
            Author = post.Author;
            Type = post.Type;
            ExchangeDetails = post.ExchangeDetails;
            ThumbUrl = post.ThumbUrl;
            IsArchived = post.IsArchived;

            // IPostDetail
            PostId = postDetail.PostId;
            Description = postDetail.Description;
            ImageUploadApiData = postDetail.ImageUploadApiData;

            // PostDetailResult
            CreatedOn = postCreatedOn;
            IsBookmarked = isBookmarked;
        }

        // IPost
        public string Title { get; init; } = default!;
        public PostAuthor? Author { get; init; }
        public PostType Type { get; init; }
        public ExchangeDetails? ExchangeDetails { get; init; }
        public string? ThumbUrl { get; init; }
        public bool IsArchived { get; init; }

        // IPostDetail
        public string PostId { get; init; } = default!;
        public string? Description { get; init; }
        public IList<ImageUploadApiData>? ImageUploadApiData { get; init; }

        // PostDetailResult
        public DateTimeOffset CreatedOn { get; init; }
        public bool IsBookmarked { get; init; }
    }
}
