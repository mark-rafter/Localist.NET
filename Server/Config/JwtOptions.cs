namespace Localist.Server.Config
{
    public interface IJwtOptions
    {
        string SecurityKey { get; init; }
        string Issuer { get; init; }
        string Audience { get; init; }
        int ExpiryInDays { get; init; }
    }

    public class JwtOptions : IJwtOptions
    {
        public string SecurityKey { get; init; } = default!;
        public string Issuer { get; init; } = default!;
        public string Audience { get; init; } = default!;
        public int ExpiryInDays { get; init; }
    }
}