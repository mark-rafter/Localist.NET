using System.Collections.Generic;

namespace Localist.Shared
{
    public record PostListResult<TPostResult>(int TotalPages, IReadOnlyList<TPostResult> PostResultList);
    public record PostResult(Post Post, long ReplyCount) : IPostResult;
    public record BookmarkedPostResult(Post Post, long ReplyCount, bool IsBookmarked, bool IsWatched) : IPostResult;

    public interface IPostResult
    {
        Post Post { get; init; }
        long ReplyCount { get; init; }
    }
}
