using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Localist.Shared;

namespace Localist.Client
{
    public interface IAuthService
    {
        Task<IAccountResult> Login(LoginModel loginModel);
        Task Logout();
        Task<IAccountResult> Register(RegisterModel registerModel);
    }

    public class AuthService : IAuthService
    {
        readonly HttpClient httpClient;
        readonly AuthenticationStateProvider authenticationStateProvider;
        readonly ILocalStorageService localStorage;
        readonly ILogger<AuthService> logger;

        public AuthService(
            HttpClient httpClient,
            AuthenticationStateProvider authenticationStateProvider,
            ILocalStorageService localStorage,
            ILogger<AuthService> logger)
        {
            this.httpClient = httpClient;
            this.authenticationStateProvider = authenticationStateProvider;
            this.localStorage = localStorage;
            this.logger = logger;
        }

        public async Task<IAccountResult> Register(RegisterModel registerModel)
        {
            var registerResult = await httpClient.PostJsonAsync<AccountResult>("api/Account/Register", registerModel);

            if (registerResult.Successful && registerResult.Token is not null)
            {
                await MarkUserAsAuthenticated(registerResult.Token);
            }

            return registerResult;
        }

        public async Task<IAccountResult> Login(LoginModel loginModel)
        {
            var loginAsJson = JsonSerializer.Serialize(loginModel);

            IAccountResult? loginResult = null;

            try
            {
                var loginResponse = await httpClient.PostAsync("api/Account/Login",
                    new StringContent(loginAsJson, System.Text.Encoding.UTF8, "application/json"));
                loginResult = await ProcessLoginResponse(loginResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login");
                return loginResult ?? new FailedAccountResult(ex.Message);
            }

            return loginResult;
        }

        async Task<IAccountResult> ProcessLoginResponse(HttpResponseMessage response)
        {
            var loginResult = JsonSerializer.Deserialize<AccountResult>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Could not deserialise login result");

            if (!response.IsSuccessStatusCode || loginResult.Error is not null) return loginResult;
            if (loginResult.Token is null) throw new InvalidOperationException("JWT auth token was null");

            await MarkUserAsAuthenticated(loginResult.Token);

            return loginResult;
        }

        async Task MarkUserAsAuthenticated(string token)
        {
            await localStorage.SetItemAsync("authToken", token);
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(token);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}