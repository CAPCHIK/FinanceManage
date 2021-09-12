using Blazored.LocalStorage;
using BuildConfiguration;
using FinanceManage.CQRS.Handlers.Client;
using FinanceManage.Site.Client.Models.Options;
using FinanceManage.Site.Shared;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Site.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.Configure<TelegramBotInfo>(tbi => builder.Configuration.GetSection(nameof(TelegramBotInfo)).Bind(tbi));
            builder.Services.Configure<BuildInfo>(bi => builder.Configuration.GetSection(nameof(BuildInfo)).Bind(bi));

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddAntDesign();
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddMediatR(typeof(AverageSpendingHandler).Assembly);


            builder.Services.AddScoped<AuthenticationStateProvider, TelegramWidgetAuthenticationProvider>();
            builder.Services.AddAuthorizationCore(options => options.AddFinanceManagePolicies());
            builder.Services.AddScoped<ILogoutService>(sp => sp.GetRequiredService<AuthenticationStateProvider>() as TelegramWidgetAuthenticationProvider);

            await builder.Build().RunAsync();
        }
    }
}
