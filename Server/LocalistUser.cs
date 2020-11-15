using AspNetCore.Identity.Mongo.Model;

namespace Localist.Server
{
    public class LocalistUser : MongoUser
    {
        public string InviteCode { get; init; }

        public LocalistUser(string userName, string inviteCode)
            : base(userName)
        {
            InviteCode = inviteCode;
        }
    }
}