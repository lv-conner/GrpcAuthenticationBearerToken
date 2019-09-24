using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelloGrpcServiceWebClient.Services
{
    public class DefaultAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private const string _tokenKey = "defaultAccessTokenProvider_access_token";
        private readonly ILogger<DefaultAccessTokenProvider> _logger;
        public DefaultAccessTokenProvider(IServiceProvider serviceProvider,IMemoryCache cache,ILogger<DefaultAccessTokenProvider> logger)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
            _logger = logger;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            string token;
            if(_cache.TryGetValue<string>(_tokenKey, out token))
            {
                return token;
            }
            else
            {
                var tokenClient = _serviceProvider.GetService<TokenClient>();
                _logger.LogInformation("Obtain Access Token");
                try
                {
                    var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync();
                    token = tokenResponse.AccessToken;
                    _cache.Set(_tokenKey, token);
                }
                catch(Exception ex)
                {
                    token = null;
                    _logger.LogError(ex, "Obtain Access Token Error");
                }
            }
            return token;
        }
    }
}
