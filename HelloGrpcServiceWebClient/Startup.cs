using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloGrpcService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using HelloGrpcServiceWebClient.Services;
using Microsoft.Extensions.Options;

namespace HelloGrpcServiceWebClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddGrpcClient<Greeter.GreeterClient>((ServiceProvider, options) =>
            {
                options.Address = new Uri("https://localhost:8888");
                options.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpClient.SetBearerToken(GetTokenAsync(ServiceProvider).Result);
                });
            });
            services.AddMemoryCache();
            services.AddTransient<IAccessTokenProvider, DefaultAccessTokenProvider>();
            services.AddHttpClient<TokenClient>(options =>
            {

            });
            services.AddTransient(sp => sp.GetRequiredService<IOptions<TokenClientOptions>>().Value);

            services.Configure<TokenClientOptions>(options =>
            {
                options.Address = "http://localhost:5000/connect/token";
                options.ClientId = "client";
                options.ClientSecret = "secret";
            });
            //当使用OpenId登陆时，将AccessToken保存在Cookie中时，可以直接使用IHttpContextAccessor获取
            //static async Task<string> GetTokenAsync(IServiceProvider serviceProvider)
            //{
            //    var context = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            //    if (context == null)
            //    {
            //        throw new ArgumentNullException(nameof(HttpContext));
            //    }
            //    return await context.GetTokenAsync("access_token");
            //}
            static async Task<string> GetTokenAsync(IServiceProvider serviceProvider)
            {
                var accessTokenProvider = serviceProvider.GetService<IAccessTokenProvider>();
                if (accessTokenProvider == null)
                {
                    throw new ArgumentNullException(nameof(IAccessTokenProvider));
                }
                return await accessTokenProvider.GetAccessTokenAsync();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
