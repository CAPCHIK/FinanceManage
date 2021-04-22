using FinanceManage.Database;
using FinanceManage.TelegramBot.Models.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;

namespace FinanceManage.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.Local.json", optional: true))
                .Build();
            ApplyMigrations(host.Services);
            host.Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.Configure<TelegramBotOptions>(configuration.GetSection(nameof(TelegramBotOptions)));

                    services.AddDbContext<FinanceManageDbContext>(options =>
                        options.UseNpgsql(configuration.GetConnectionString("Database")));

                    services.AddAutoMapper(typeof(Program).Assembly);

                    services.AddMediatR(typeof(Program).Assembly);

                    services.AddHostedService<Worker>();


                    services.AddHttpClient(nameof(ITelegramBotClient))
                        .AddPolicyHandler(GetRetryPolicy());

                    services.AddSingleton<ITelegramBotClient>((sp) =>
                    {
                        var botOptions = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
                        var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ITelegramBotClient));
                        return new TelegramBotClient(botOptions.AccessToken, httpClient: httpClient);
                    });
                });

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var random =  new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(
                    random.NextDouble() * Math.Pow(2, retryAttempt)));
        }

        private static void ApplyMigrations(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<FinanceManageDbContext>();
            db.Database.Migrate();
        }
    }
}
