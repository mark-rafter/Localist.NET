using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using Localist.Server.Helpers;
using Localist.Shared;

namespace Localist.Server.Services
{
    public interface IInviteService
    {
        Task AddLostCode(string address);
        Task<Invite?> FindInvite(string code);
        Task<Invite> UpdateInvite(Invite invite);
    }

    public class InviteService : IInviteService
    {
        readonly IDbContext dbContext;

        public InviteService(IDbContext dbContext)
            => this.dbContext = dbContext;

        public Task AddLostCode(string address)
            => dbContext.LostCodes.Upsert(new LostCode(address));

        public async Task<Invite?> FindInvite(string code)
            => (await dbContext.Invites.FindAsync(i => i.Code == code && i.Username == null)).SingleOrDefault();

        public Task<Invite> UpdateInvite(Invite invite)
            => dbContext.Invites.Upsert(invite);
    }
}
