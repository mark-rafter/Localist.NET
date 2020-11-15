namespace Localist.Shared
{
    public class ImageUploadApiResult
    {
        public ImageUploadApiData Data { get; set; } = default!;
        public bool Success { get; set; } = default!;
        public int Status { get; set; } = default!;
    }

    public class ImageUploadApiFile
    {
        public string Url { get; set; } = default!;
    }

    public class ImageUploadApiData
    {
        public string Id { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string Time { get; set; } = default!;
        public ImageUploadApiFile Thumb { get; set; } = default!;
        public string Delete_Url { get; set; } = default!;
    }
}