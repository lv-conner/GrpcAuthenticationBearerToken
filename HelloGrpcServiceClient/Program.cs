using Grpc.Net.Client;
using HelloGrpcService;
using System;
using System.Net.Http;
using IdentityModel;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HelloGrpcServiceClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var actoken =await GetAccessTokenAsync();
            Console.WriteLine(actoken);



            // request token
            var tokenClient = new TokenClient(new HttpClient() { BaseAddress = new Uri("http://localhost:5000/connect/token") }, new TokenClientOptions()
            {
                ClientId = "client",
                ClientSecret = "secret"
            });
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync();
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }
            Console.WriteLine(tokenResponse.Json);
            IServiceCollection services = new ServiceCollection();
            services.AddGrpcClient<Greeter.GreeterClient>((ServiceProvider,options) =>
            {
                options.Address = new Uri("https://localhost:8888");
                options.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpClient.SetBearerToken(GetTokenAsync(ServiceProvider).Result);
                });
            });
            var provider = services.BuildServiceProvider();
            var serviceClient = provider.GetService<Greeter.GreeterClient>();
            var res = await serviceClient.SayHelloAsync(new HelloRequest() { Name = "tim" });
            Console.WriteLine(res.Message);


            static async Task<string> GetTokenAsync(IServiceProvider serviceProvider)
            {
                var context = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
                if(context == null)
                {
                    throw new ArgumentNullException(nameof(HttpContext));
                }
                return await context.GetTokenAsync("access_token");
            }

            var httpClient = new HttpClient();
            httpClient.SetBearerToken(tokenResponse.AccessToken);
            var channel = GrpcChannel.ForAddress("https://localhost:8888", new GrpcChannelOptions()
            {
                HttpClient = httpClient
            }); 
            var client = new Greeter.GreeterClient(channel);
            var helloReply = await client.SayHelloAsync(new HelloRequest() { Name = "tim lv" });
            Console.WriteLine(helloReply.Message);
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        static async Task<string> GetAccessTokenAsync()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient<TokenClient>(options =>
            {
                //options.BaseAddress = new Uri("http://localhost:5000/connect/token");
            });
            services.Configure<TokenClientOptions>(options =>
            {
                options.Address = "http://localhost:5000/connect/token";
                options.ClientId = "client";
                options.ClientSecret = "secret";
            });
            services.AddTransient(sp => sp.GetRequiredService<IOptions<TokenClientOptions>>().Value);
            var provider = services.BuildServiceProvider();
            var client = provider.GetService<TokenClient>();
            var response = await client.RequestClientCredentialsTokenAsync();
            Console.WriteLine(response.AccessToken);
            return response.AccessToken;
        }
    }
}
