using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Localist.Shared;

namespace Localist.Client.Pages
{
    public class BookmarksBase : ComponentBase
    {
        [Inject]
        public HttpClient Http { get; init; } = default!;

        [Parameter]
        public int? CurrentPage { get; set; }

        public BookmarkedPostResult[]? PostResults { get; set; }

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
            if (await Http.GetFromJsonAsync<PostListResult<BookmarkedPostResult>>($"api/Post/bookmarks/{CurrentPage}")
                is PostListResult<BookmarkedPostResult> result)
            {
                TotalPages = result.TotalPages;
                PostResults = result.PostResultList.ToArray();
            }
            else
            {
                Error = "Failed to retrieve bookmarks";
            }
        }
    }
}