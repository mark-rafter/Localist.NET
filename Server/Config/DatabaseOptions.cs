namespace Localist.Server.Config
{
    public interface IDatabaseOptions
    {
        string Name { get; init; }
        string ConnectionString { get; init; }
    }

    public class DatabaseOptions : IDatabaseOptions
    {
        public string Name { get; init; } = default!;
        public string ConnectionString { get; init; } = default!;
    }
}