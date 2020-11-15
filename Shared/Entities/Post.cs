namespace Localist.Shared
{
    public interface IPost
    {
        string Title { get; init; }
        PostAuthor? Author { get; init; }
        PostType Type { get; init; }
        ExchangeDetails? ExchangeDetails { get; init; }
        string? ThumbUrl { get; init; }
        bool IsArchived { get; init; }
    }

    public record Post(
        string Title,
        PostAuthor? Author = null,
        PostType Type = PostType.Message,
        ExchangeDetails? ExchangeDetails = null,
        string? ThumbUrl = null,
        bool IsArchived = false
    ) : DbEntity, IPost
    {
        // public string Url => $"{new string(Title.Take(10).ToArray()).Replace(" ", "-")}-{new string(Id.Take(10).ToArray())}";
    }
}
