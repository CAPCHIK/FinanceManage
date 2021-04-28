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
        private bool saved;
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
            if (!saved)
            {
                var selfRef = DotNetObjectReference.Create(this);
                await jsRuntime.InvokeVoidAsync("saveAuthReference", selfRef);
                saved = true;
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
                    if (await TryLoginAsUser(telegramUserInfo))
                    {
                        return TelegramWidgetClaimsGenerator.GetPrincipal(telegramUserInfo);
                    }
                    else
                    {
                        Console.WriteLine("can't login");

                        return new ClaimsPrincipal();
                    }
                }
            }
            catch
            {
                Console.WriteLine("exception");
                return new ClaimsPrincipal();
            }
        }

        private async Task<bool> TryLoginAsUser(TelegramUserInfo user)
        {
            var userInfoJson = JsonSerializer.Serialize(user);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("TelegramWidget",
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userInfoJson)));
            var response = await httpClient.PostAsJsonAsync("/api/auth", user);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await localStorage.RemoveItemAsync("telegramUserInfo");
                await localStorage.SetItemAsync("telegramUserInfo", user);
                return true;
            }
            return false;
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
