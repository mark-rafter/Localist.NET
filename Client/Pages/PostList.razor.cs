using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Localist.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;

namespace Localist.Client.Pages
{
    public class PostListBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Parameter]
        public int? CurrentPage { get; set; }

        public PostResult[]? PostResults { get; set; }

        public string? Error { get; set; }

        public int? TotalPages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentPage ??= 1;
            await PopulatePosts();
        }

        async Task PopulatePosts()
        {
            // todo: cache/offline-storage (see stash)
            if (await Http.GetFromJsonAsync<PostListResult<PostResult>>($"api/Post/{CurrentPage}") is PostListResult<PostResult> result)
            {
                TotalPages = result.TotalPages;
                PostResults = result.PostResultList.ToArray();
            }
            else
            {
                Error = "Failed to retrieve posts";
            }
        }
    }
}
