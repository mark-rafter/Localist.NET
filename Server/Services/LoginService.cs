using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface ILoginService
    {
        Task<IAccountResult> Login(string username, string password, bool rememberMe);
    }

    public class LoginService : ILoginService
    {
        readonly IJwtService jwtService;
        readonly SignInManager<LocalistUser?> signInManager;

        public LoginService(
            IJwtService jwtService, 
            SignInManager<LocalistUser?> signInManager)
        {
            this.jwtService = jwtService;
            this.signInManager = signInManager;
        }

        public async Task<IAccountResult> Login(string username, string password, bool rememberMe)
        {
            var result = await signInManager.PasswordSignInAsync(
                username, password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded) 
                return new FailedAccountResult("Invalid credentials.");

            if (result.IsLockedOut) 
                return new FailedAccountResult("Too many incorrect attempts, try again in 5 minutes.");

            var jwt = await jwtService.GenerateJwtToken(username, rememberMe);

            return new SuccessfulAccountResult(jwt);
        }
    }
}
