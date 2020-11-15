namespace Localist.Shared
{
    public record Invite(string Code, string? Username) : DbEntity;
}
