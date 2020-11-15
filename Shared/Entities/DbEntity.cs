using System;

namespace Localist.Shared
{
    public interface IDbEntity
    {
        string? Id { get; init; }
        string? ShortId { get; }
        DateTimeOffset? CreatedOn { get; init; }
        DateTimeOffset? ModifiedOn { get; init; }
    }

    public abstract record DbEntity : IDbEntity
    {
        public string? Id { get; init; }

        public string? ShortId => Id?[^8..];

        public DateTimeOffset? CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; init; }

        public DbEntity() { }
    }
}
