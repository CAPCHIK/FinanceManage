using FinanceManage.TelegramBot.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace FinanceManage.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.Local.json", optional: true))
                .Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.Configure<TelegramBotOptions>(configuration.GetSection(nameof(TelegramBotOptions)));
                    services.AddHostedService<Worker>();

                    services.AddSingleton<ITelegramBotClient>((sp) =>
                    {
                        var botOptions = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
                        return new TelegramBotClient(botOptions.AccessToken);
                    });
                });
    }
}
