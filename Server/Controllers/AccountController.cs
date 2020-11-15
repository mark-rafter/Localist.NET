using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Localist.Server.Services;
using Localist.Shared;

namespace Localist.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        readonly IAccountService accountService;
        readonly IInviteService inviteService;
        readonly ILoginService loginService;

        public AccountController(
            IAccountService accountService,
            IInviteService inviteService,
            ILoginService loginService)
        {
            this.accountService = accountService;
            this.inviteService = inviteService;
            this.loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<IAccountResult>> Login([FromBody] LoginModel loginModel)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            var loginResult = await loginService.Login(
                loginModel.Username,
                loginModel.Password,
                loginModel.RememberMe);

            return loginResult switch
            {
                FailedAccountResult failedLoginResult => Unauthorized(failedLoginResult),
                SuccessfulAccountResult successfulLoginResult => Ok(successfulLoginResult),
                _ => throw new NotImplementedException($"Unknown {nameof(loginResult)} type: {loginResult.GetType().FullName}")
            };
        }

        [HttpPost("lost-code")]
        public async Task<IActionResult> LostCode([FromBody] LostCodeModel model)
        {
            // todo: log IP hash
            // var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            await inviteService.AddLostCode(model.Address);
            return NoContent();
        }

        [HttpPost("register")]
        public async Task<ActionResult<IAccountResult>> RegisterAndLogin([FromBody] RegisterModel registerModel)
        {
            var registerResult = await accountService.Register(registerModel);

            if (registerResult is FailedAccountResult failedRegisterResult)
            {
                return Ok(failedRegisterResult);
            }
            else if (registerResult.Successful)
            {
                return await Login(new LoginModel
                {
                    Username = registerModel.Username,
                    Password = registerModel.Password
                });
            }
            else
            {
                throw new NotImplementedException(
                    $"Unknown {nameof(registerResult)} type: {registerResult.GetType().FullName}");
            }
        }
    }
}
