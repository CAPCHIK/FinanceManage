using FinanceManage.CQRS.Handlers.Server;
using FinanceManage.Database;
using FinanceManage.Models.ServerSide.Options;
using FinanceManage.Telegram.Shared;
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

                    services.AddAutoMapper(typeof(Program).Assembly, typeof(GetAverageSpendingPerDayHandler).Assembly);

                    services.AddMediatR(typeof(Program).Assembly, typeof(GetAverageSpendingPerDayHandler).Assembly);

                    services.AddHostedService<Worker>();

                    services.AddTelegramBotClient();
                });


        private static void ApplyMigrations(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<FinanceManageDbContext>();
            db.Database.Migrate();
        }
    }
}
