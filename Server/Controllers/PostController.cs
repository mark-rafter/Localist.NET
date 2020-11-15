using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Localist.Server.Services;
using Localist.Shared;
using Localist.Shared.Helpers;

namespace Localist.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        readonly IProfileService profileService;        
        readonly IPostService postService;
        readonly ILogger<PostController> logger;

        public PostController(
            IProfileService profileService,
            IPostService postService,
            ILogger<PostController> logger)
        {
            this.profileService = profileService;
            this.postService = postService;
            this.logger = logger;
        }

        [HttpGet("{page:int:min(1)}")]
        public async Task<ActionResult<PostListResult<PostResult>>> Get(int page)
        {
            return Ok(await postService.GetPostList(page));
        }

        [HttpGet("bookmarks/{page:int:min(1)}")]
        public async Task<ActionResult<PostListResult<BookmarkedPostResult>>> GetBookmarkedPosts(int page)
        {
            var profile = await profileService.GetProfile(User.GetUserId());

            if (!profile.BookmarkIds.Any() && !profile.WatchIds.Any())
                return Ok(new PostListResult<BookmarkedPostResult>(0, new List<BookmarkedPostResult>()));

            var result = await postService.GetPostList(page, profile.BookmarkIds, profile.WatchIds);

            return Ok(result);
        }

        [HttpGet("detail/{id:length(24)}")]
        public async Task<IActionResult> GetPostDetail(string id)
        {
            if (await postService.GetPostDetail(id) is PostDetail postDetail
                && await postService.GetPost(id) is Post post)
            {
                var isBookmarked = await profileService.IsEntityBookmarked(User.GetUserId(), id);
                var result = new PostDetailResult(post, postDetail, post.CreatedOn!.Value, isBookmarked);
                return Ok(result);
            }

            return NotFound();
        }

        [HttpGet("replies/{id:length(24)}")]
        public async Task<IActionResult> GetReplyTrees(string id)
        {
            var result = await postService.GetReplyTrees(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(NewPostModel newPostModel)
        {
            var result = await postService.AddPost(newPostModel, User);

            await profileService.AddBookmark(
                User.GetUserId(),
                result.PostId,
                newPostModel.EnableNotifications);

            return Ok(result.PostId); // todo?: return Created()
        }

        [HttpPost("reply")]
        public async Task<IActionResult> AddReply(NewPostReplyModel newPostReplyModel)
        {
            var result = await postService.AddReply(newPostReplyModel, User);
            return Ok(result); // todo?: return Created()
        }

        /// <remarks>This is a PATCH operation, not PUT</remarks>
        [HttpPut("add")]
        public async Task<IActionResult> PatchAdd([FromBody] PatchPostModel postModel)
        {
            if (postModel.IsArchived == true)
            {
                await postService.ArchivePost(postModel.Id, User.GetUserId(), true);
                return NoContent();
            }

            return BadRequest(ModelState);
        }

        /// <remarks>This is a PATCH operation, not PUT</remarks>
        [HttpPut("remove")]
        public async Task<IActionResult> PatchRemove([FromBody] PatchPostModel postModel)
        {
            if (postModel.IsArchived == false)
            {
                await postService.ArchivePost(postModel.Id, User.GetUserId(), false);
                return NoContent();
            }

            return BadRequest(ModelState);
        }
    }
}
