using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Localist.Server.Services;
using Localist.Shared;
using Localist.Shared.Helpers;

namespace Localist.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        readonly INotificationService notificationService;
        readonly IProfileService profileService;
        readonly ILogger<ProfileController> logger;

        public ProfileController(
            INotificationService notificationService,
            IProfileService profileService,
            ILogger<ProfileController> logger)
        {
            this.notificationService = notificationService;
            this.profileService = profileService;
            this.logger = logger;
        }

        /// <remarks>This is a PATCH operation, not PUT</remarks>
        [HttpPut("add")]
        public async Task<IActionResult> PatchAdd([FromBody] PatchProfileModel profileModel)
        {
            await profileService.AddToProfile(User.GetUserId(), profileModel);
            return NoContent();
        }

        /// <remarks>This is a PATCH operation, not PUT</remarks>
        [HttpPut("remove")]
        public async Task<IActionResult> PatchRemove([FromBody] PatchProfileModel profileModel)
        {
            await profileService.RemoveFromProfile(User.GetUserId(), profileModel);
            return NoContent();
        }

        [HttpPut("notification-subscription")]
        public async Task<IActionResult> AddNotificationSubscription(NotificationSubscription subscription)
        {
            string userAgent = HttpContext.Request.Headers.TryGetValue("User-Agent", out var browser) ? browser : "";

            await notificationService.Subscribe(
                subscription with { UserAgent = userAgent },
                User.GetUserId());

            return NoContent();
        }
    }
}