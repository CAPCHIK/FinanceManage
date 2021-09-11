using FinanceManage.Models.ServerSide.Options;
using FinanceManage.Site.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.LoginWidget;

namespace FinanceManage.Site.Server.Authentication
{
    public class TelegramWidgetOptions : AuthenticationSchemeOptions { }
    public class TelegramWidgetAuthenticationHandler : AuthenticationHandler<TelegramWidgetOptions>
    {
        private readonly IOptions<TelegramBotOptions> telegramBotOptions;
        private readonly InternalClaimsIdentityGenerator internalClaimsIdentityGenerator;

        public TelegramWidgetAuthenticationHandler(
            IOptions<TelegramBotOptions> telegramBotOptions,
            InternalClaimsIdentityGenerator internalClaimsIdentityGenerator,
            IOptionsMonitor<TelegramWidgetOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this.telegramBotOptions = telegramBotOptions;
            this.internalClaimsIdentityGenerator = internalClaimsIdentityGenerator;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(HandleAuthenticateSync());
        }

        protected AuthenticateResult HandleAuthenticateSync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var headerRow))
            {
                return AuthenticateResult.NoResult();
            }
            if (!AuthenticationHeaderValue.TryParse(headerRow.ToString(), out var header))
            {
                return AuthenticateResult.NoResult();
            }
            if (header.Scheme != "TelegramWidget")
            {
                return AuthenticateResult.NoResult();
            }

            byte[] infoInBase64;
            try
            {
                infoInBase64 = Convert.FromBase64String(header.Parameter);
            }
            catch
            {
                return AuthenticateResult.Fail($"invalid base64 content");
            }

            var jsonParam = Encoding.UTF8.GetString(infoInBase64);
            TelegramUserInfo userInfo;
            try
            {
                userInfo = JsonSerializer.Deserialize<TelegramUserInfo>(jsonParam);
            }
            catch
            {
                return AuthenticateResult.Fail($"invalid json content in base64 string");
            }

            var loginWidget = new LoginWidget(telegramBotOptions.Value.AccessToken)
            {
                AllowedTimeOffset = (long)TimeSpan.FromDays(10).TotalSeconds
            };
            var userInfoAsDictionary = ReadUserInfoAsDictionary(userInfo);

            var authResult = loginWidget.CheckAuthorization(userInfoAsDictionary);

            if (authResult != Authorization.Valid)
            {
                return AuthenticateResult.Fail($"Incorrect telegram info: {authResult}");
            }
            var tgIdentity = TelegramWidgetClaimsIdentityGenerator.GetIdentityForUserInfo(userInfo);
            var internalPrincipal = internalClaimsIdentityGenerator.Generate(userInfo);

            var principal = new ClaimsPrincipal(new ClaimsIdentity[] { tgIdentity, internalPrincipal });
            return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationSchemeConstants.TelegramWidgetAuthenticationScheme));
        }

        private static Dictionary<string, string> ReadUserInfoAsDictionary(TelegramUserInfo userInfo)
        {
            return userInfo.GetType()
                .GetProperties()
                .Select(p => (key: p.GetCustomAttribute<JsonPropertyNameAttribute>().Name, value: p.GetValue(userInfo)))
                .Where(p => p.value != null)
                .ToDictionary(p => p.key, p => p.value?.ToString());
        }
    }
}
