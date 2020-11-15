namespace Localist.Shared
{
    public record PostReply(
        string PostId,
        string? ParentId,
        string Message,
        PostAuthor? Author = null
    ) : DbEntity;
}
