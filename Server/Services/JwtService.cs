using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Localist.Server.Config;

namespace Localist.Server.Services
{
    public interface IJwtService
    {
        Task<string> GenerateJwtToken(string username, bool rememberMe);
    }

    public class JwtService : IJwtService
    {
        readonly IJwtOptions jwtOptions;
        readonly UserManager<LocalistUser?> userManager;

        public JwtService(
            IJwtOptions jwtOptions,
            UserManager<LocalistUser?> userManager)
        {
            this.jwtOptions = jwtOptions;
            this.userManager = userManager;
        }

        public async Task<string> GenerateJwtToken(string username, bool rememberMe)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(rememberMe ? Convert.ToInt32(jwtOptions.ExpiryInDays) : 1);

            var user = await userManager.FindByNameAsync(username)
                ?? throw new InvalidOperationException($"User {username} does not exist");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, username),
            };

            var token = new JwtSecurityToken(
                jwtOptions.Issuer,
                jwtOptions.Audience,
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
