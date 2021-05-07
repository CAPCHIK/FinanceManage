using FinanceManage.Models.ServerSide.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using Telegram.Bot;

namespace FinanceManage.Telegram.Shared
{
    public static class AddTelegramClientExtension
    {
        public static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
        {
            services.AddHttpClient(nameof(ITelegramBotClient))
                       .AddPolicyHandler(GetRetryPolicy());

            services.AddSingleton<ITelegramBotClient>((sp) =>
            {
                var botOptions = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ITelegramBotClient));
                return new TelegramBotClient(botOptions.AccessToken, httpClient: httpClient);
            });
            return services;
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var random = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(
                    random.NextDouble() * Math.Pow(2, retryAttempt)));
        }
    }
}
