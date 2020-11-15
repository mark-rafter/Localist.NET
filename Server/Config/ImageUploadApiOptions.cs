namespace Localist.Server.Config
{
    public interface IImageUploadApiOptions
    {
        string Url { get; init; }
        string ApiKey { get; init; }
    }

    public class ImageUploadApiOptions : IImageUploadApiOptions
    {
        public string Url { get; init; } = default!;
        public string ApiKey { get; init; } = default!;
    }
}