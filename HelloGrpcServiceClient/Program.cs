using Grpc.Net.Client;
using HelloGrpcService;
using System;
using System.Net.Http;
using IdentityModel;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace HelloGrpcServiceClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
    }
}
