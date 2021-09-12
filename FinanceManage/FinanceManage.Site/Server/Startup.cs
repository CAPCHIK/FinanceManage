using AspNetCore.Proxy;
using FinanceManage.CQRS.Handlers.Server;
using FinanceManage.Database;
using FinanceManage.Models.ServerSide.Options;
using FinanceManage.Site.Server.Authentication;
using FinanceManage.Site.Shared;
using FinanceManage.Telegram.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace FinanceManage.Site.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private bool IsNeedForwardingApi => Configuration.GetValue<bool>("FOWRAWD_API");
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TelegramBotOptions>(Configuration.GetSection(nameof(TelegramBotOptions)));
            services.Configure<AdministrationOptions>(Configuration.GetSection(nameof(AdministrationOptions)));
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddDbContext<FinanceManageDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Database")));

            services.AddAutoMapper(typeof(GetAverageSpendingPerDayHandler).Assembly);

            services.AddMediatR(typeof(GetAverageSpendingPerDayHandler).Assembly);

            services.AddTelegramBotClient();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme
                    = AuthenticationSchemeConstants.TelegramWidgetAuthenticationScheme;
            })
            .AddScheme<TelegramWidgetOptions, TelegramWidgetAuthenticationHandler>
                    (AuthenticationSchemeConstants.TelegramWidgetAuthenticationScheme, op => { });

            services.AddAuthorizationCore(options => options.AddFinanceManagePolicies());

            services.AddSingleton<InternalClaimsIdentityGenerator>();

            if (IsNeedForwardingApi)
            {
                services.AddProxies();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                if (IsNeedForwardingApi)
                {
                    // Mock puchases info from real personal data
                    app.UseProxies(proxies => proxies.Map("api/purchases/{id}", proxy => proxy.UseHttp((context, args) =>
                    {
                        return $"http://127.0.0.1.nip.io/devdata/purchases.json";
                    })));
                }
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
