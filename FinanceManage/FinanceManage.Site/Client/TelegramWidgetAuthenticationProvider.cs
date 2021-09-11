using Blazored.LocalStorage;
using FinanceManage.Site.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace FinanceManage.Site.Client
{
    public class TelegramWidgetAuthenticationProvider : AuthenticationStateProvider, ILogoutService
    {
        private readonly IJSRuntime jsRuntime;
        private readonly ILocalStorageService localStorage;
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private bool callbackReferenceSaved;
        public TelegramWidgetAuthenticationProvider(
            IJSRuntime jsRuntime,
            ILocalStorageService localStorage,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            this.jsRuntime = jsRuntime;
            this.localStorage = localStorage;
            this.configuration = configuration;
            this.httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!callbackReferenceSaved)
            {
                var selfRef = DotNetObjectReference.Create(this);
                await jsRuntime.InvokeVoidAsync("saveAuthReference", selfRef);
                callbackReferenceSaved = true;
            }
            var principal = await GetPrincipal();
            return new AuthenticationState(principal);

        }

        private async Task<ClaimsPrincipal> GetPrincipal()
        {
            var version = await localStorage.GetItemAsync<string>("version");
            var correctVersion = configuration.GetSection("Version").Value;
            if (version != correctVersion)
            {
                await localStorage.ClearAsync();
                await localStorage.SetItemAsync(nameof(version), correctVersion);
            }
            try
            {
                var telegramUserInfo = await localStorage.GetItemAsync<TelegramUserInfo>("telegramUserInfo");
                if (telegramUserInfo == null)
                {
                    Console.WriteLine("no user info");
                    return new ClaimsPrincipal();
                }
                else
                {
                    var (success, claims) = await TryGetInternalClaims(telegramUserInfo);
                    if (success)
                    {
                        var tgIdentity = TelegramWidgetClaimsIdentityGenerator.GetIdentityForUserInfo(telegramUserInfo);
                        var internalIdentity = new ClaimsIdentity(claims, InternalClaimConstants.IDENTITY_AUTH_TYPE);

                        return new ClaimsPrincipal(new ClaimsIdentity[] { tgIdentity, internalIdentity });
                    }
                    else
                    {
                        Console.WriteLine("can't login");

                        return new ClaimsPrincipal();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception");
                Console.WriteLine(ex);
                return new ClaimsPrincipal();
            }
        }

        private async Task<(bool, Claim[])> TryGetInternalClaims(TelegramUserInfo user)
        {
            var userInfoJson = JsonSerializer.Serialize(user);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("TelegramWidget",
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userInfoJson)));
            var response = await httpClient.PostAsync("/api/auth", null);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await localStorage.RemoveItemAsync("telegramUserInfo");
                await localStorage.SetItemAsync("telegramUserInfo", user);

                var responseText = await response.Content.ReadAsStringAsync();
                var claimModels = JsonSerializer.Deserialize<ClaimModel[]>(responseText, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                var claims = claimModels.Select(c => new Claim(c.Type, c.Value)).ToArray();
                return (true, claims);
            }
            return (false, default);
        }
        [JSInvokable]
        public async Task UserAuthCallback(TelegramUserInfo userInfo)
        {
            await localStorage.RemoveItemAsync("telegramUserInfo");
            await localStorage.SetItemAsync("telegramUserInfo", userInfo);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("telegramUserInfo");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
