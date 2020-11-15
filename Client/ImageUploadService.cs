using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace Localist.Client
{
    public interface IImageUploadService
    {
        IAsyncEnumerable<string> UploadFilesAsync(IReadOnlyList<IBrowserFile> files, CancellationToken ct = default);
        string? Error { get; set; }
    }

    public class ImageUploadService : IImageUploadService
    {
        const int maxFileSize = 1_500_000;
        const string format = "image/jpeg";
        readonly string[] allowedMimeTypes = new[] { format, "image/png" };

        public string? Error { get; set; }

        public async IAsyncEnumerable<string> UploadFilesAsync(IReadOnlyList<IBrowserFile> files, [EnumeratorCancellation] CancellationToken ct = default)
        {
            Error = null;
            foreach (var file in files)
            {
                if (!allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    Error = $"File {file.Name} has type {file.ContentType}. Must be one of the following types: {string.Join(", ", allowedMimeTypes)}";
                    continue;
                }

                var resizedImageFile = await file.RequestImageFileAsync(format, 720, 1080);

                if (resizedImageFile.Size > maxFileSize)
                {
                    Error = $"File {file.Name} is too large. Must be less than {maxFileSize / 1_000_000} MB";
                    continue;
                }

                yield return await ConvertFileToBase64Async(resizedImageFile, ct);
            }
        }

        static async Task<string> ConvertFileToBase64Async(IBrowserFile file, CancellationToken ct = default)
        {
            var buffer = new byte[file.Size];

            await file
                .OpenReadStream(maxFileSize, ct)
                .ReadAsync(buffer, ct);

            return Convert.ToBase64String(buffer);
        }
    }
}