using System.Collections.Generic;

namespace Localist.Shared
{
    public interface IPostDetail
    {
        string PostId { get; init; }
        string? Description { get; init; }
        IList<ImageUploadApiData>? ImageUploadApiData { get; init; }
    }

    public record PostDetail(
        string PostId,
        string? Description = null,
        IList<ImageUploadApiData>? ImageUploadApiData = null
    ) : DbEntity, IPostDetail;
}
