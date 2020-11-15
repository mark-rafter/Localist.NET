using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface IAccountService
    {
        Task<IAccountResult> Register(RegisterModel model);
    }

    public class AccountService : IAccountService
    {
        readonly IProfileService profileService;
        readonly IInviteService inviteService;
        readonly UserManager<LocalistUser?> userManager;

        public AccountService(
            IProfileService profileService,
            IInviteService inviteService,
            UserManager<LocalistUser?> userManager)
        {
            this.profileService = profileService;
            this.inviteService = inviteService;
            this.userManager = userManager;
        }

        public async Task<IAccountResult> Register(RegisterModel model)
        {
            // todo: improve ACIDity
            var invite = await inviteService.FindInvite(model.InviteCode);

            if (invite is null)
                return new FailedAccountResult($"Invalid invite code: {model.InviteCode}");

            if (await userManager.FindByNameAsync(model.Username) is not null)
                return new FailedAccountResult("Username already taken");

            var newUser = new LocalistUser(model.Username, model.InviteCode);

            var createUserResult = await userManager.CreateAsync(newUser, model.Password);

            if (!createUserResult.Succeeded)
                return new FailedAccountResult(string.Join(", ", createUserResult.Errors.Select(x => x.Description)));

            await UpdateInviteAndCreateProfileForUser(model.Username, invite);

            return new AccountResult(Successful: true);
        }

        async ValueTask UpdateInviteAndCreateProfileForUser(string username, Invite invite)
        {
            var user = await userManager.FindByNameAsync(username)
                ?? throw new InvalidOperationException($"User {username} does not exist");

            await inviteService.UpdateInvite(invite with { Username = username });

            await profileService.Create(user.Id.ToString());
        }
    }
}
