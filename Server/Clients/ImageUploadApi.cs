using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Localist.Server.Config;
using Localist.Shared;

namespace Localist.Server.Clients
{
    public interface IImageUploadApi
    {
        Task<ImageUploadApiData> Upload(string base64);
    }

    public class ImageUploadApi : IImageUploadApi
    {
        readonly HttpClient http;
        readonly IImageUploadApiOptions apiOptions;

        public ImageUploadApi(HttpClient http, IImageUploadApiOptions apiOptions)
        {
            this.apiOptions = apiOptions;
            this.http = http;
            this.http.BaseAddress = new Uri(apiOptions.Url);
        }

        public async Task<ImageUploadApiData> Upload(string base64)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string?, string?>("image", base64)
            });

            using var httpResponse = await http.PostAsync($"upload?key={apiOptions.ApiKey}", formContent);

            httpResponse.EnsureSuccessStatusCode();

            var responseContent = await httpResponse.Content.ReadFromJsonAsync<ImageUploadApiResult>()
                ?? throw new InvalidOperationException("Failed to deserialise response");

            return responseContent.Data;
        }
    }
}
