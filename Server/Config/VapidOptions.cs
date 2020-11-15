namespace Localist.Server.Config
{
    public interface IVapidOptions
    {
        string Subject { get; init; }
        string PublicKey { get; init; }
        string PrivateKey { get; init; }
    }

    public class VapidOptions : IVapidOptions
    {
        public string Subject { get; init; } = default!;
        public string PublicKey { get; init; } = default!;
        public string PrivateKey { get; init; } = default!;
    }
}