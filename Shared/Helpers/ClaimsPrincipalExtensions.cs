using System;
using System.Security.Claims;

namespace Localist.Shared.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
            => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User has no id");

        public static string GetUserName(this ClaimsPrincipal principal)
            => principal.FindFirst(ClaimTypes.Name)?.Value
                ?? throw new InvalidOperationException("User has no name");
    }
}