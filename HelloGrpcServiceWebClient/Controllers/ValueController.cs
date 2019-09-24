using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloGrpcService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelloGrpcServiceWebClient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValueController : ControllerBase
    {
        private readonly Greeter.GreeterClient _client;
        public ValueController(Greeter.GreeterClient client)
        {
            _client = client;
        }
        [HttpGet]
        public async Task<ActionResult<string>> GetValue()
        {
            var res = await _client.SayHelloAsync(new HelloRequest() { Name = "tim lv" });
            return res.Message;
        }
    }
}