using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloGrpcServiceWebClient.Services
{
    public interface IAccessTokenProvider
    {
        Task<string> GetAccessTokenAsync();
    }
}
