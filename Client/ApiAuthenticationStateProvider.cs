using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Localist.Client
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        readonly HttpClient httpClient;
        readonly ILocalStorageService localStorage;

        static readonly ClaimsPrincipal anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(anonymousUser);
            }

            var claims = ParseClaimsFromJwt(savedToken);

            if (IsJwtValid(claims))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
            }

            return new AuthenticationState(anonymousUser);
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        static bool IsJwtValid(IList<Claim> claims)
        {
            return claims.SingleOrDefault(c => c.Type == "exp") is Claim expiryClaim
                && long.TryParse(expiryClaim.Value, out long expiryUnixTime)
                && DateTimeOffset.FromUnixTimeSeconds(expiryUnixTime) > DateTimeOffset.Now;
        }

        static IList<Claim> ParseClaimsFromJwt(string jwt)
        {
            var tokenSegments = jwt.Split('.');

            if (tokenSegments.Length != 3) throw new ArgumentException("JWT Token should have three segments");

            var payload = tokenSegments[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)
                ?? throw new InvalidOperationException($"Could not deserialise JWT: {jwt}");

            var claims = new List<Claim>();

            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles) && roles is JsonElement jsonRoles)
            {
                if (jsonRoles.ValueKind == JsonValueKind.Array)
                    claims.AddRange(jsonRoles.EnumerateArray().Select(role => new Claim(ClaimTypes.Role, role.GetString()!)));
                else
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            //add other claims as well
            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
            return claims;
        }

        static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
