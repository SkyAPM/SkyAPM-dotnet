using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using GrpcGreeter;
using Microsoft.AspNetCore.Mvc;
using SkyApm.Sample.Backend.Services;
using SkyApm.Sample.Frontend.Models;

#if NETCOREAPP2_1
#else
using Grpc.Net.Client;
#endif

namespace SkyApm.Sample.Frontend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly GreeterGrpcService _greeter;

        public ValuesController(GreeterGrpcService greeter)
        {
            _greeter = greeter;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await new HttpClient().GetAsync("http://localhost:5002/api/values");
            return new string[] { "value1", "value2" };
        }

        [HttpGet("postin")]
        public async Task<string> PostIn()
        {
            var result = await new HttpClient().PostAsync("http://localhost:5002/api/values/postin", JsonContent.Create(new { Name = "SkyApm" }));
            return await result.Content.ReadAsStringAsync();
        }

        [HttpGet("{id}")]
        public async Task<string> Get(int id)
        {
            var client = new HttpClient();
            Task.WhenAll(client.GetAsync("http://localhost:5002/api/delay/2000"),
                client.GetAsync("http://localhost:5002/api/values"),
                client.GetAsync("http://localhost:5002/api/delay/200"));
            return await client.GetStringAsync("http://localhost:5002/api/delay/100");
        }

        [HttpGet("greeter")]
        public async Task<IActionResult> SayHelloAsync(string name)
        {
            var content = new StringBuilder();
            var message = await _greeter.SayHelloAsync(name);
            content.AppendLine($"from frontend grpc message:{message}");

            var response = await new HttpClient().GetStringAsync("http://localhost:5002/api/values");
            content.AppendLine($"from frontend httpclient message:{response}");

            response = await new HttpClient().GetStringAsync($"http://localhost:5002/api/grpc/greeter?name={name}");
            content.AppendLine(response);

            return Ok(content.ToString());
        }

        [HttpGet("greeter/exception")]
        public async Task<IActionResult> SayHelloWithExceptionAsync(string name)
        {
            var message = await _greeter.SayHelloWithExceptionAsync(name);
            return Ok(message);
        }

        [HttpGet("hellojava")]
        public async Task<IActionResult> HelloJava()
        {
            var message = await new HttpClient().GetStringAsync("http://localhost:8086/sayhello");
            return Ok(message);
        }

        [HttpGet("setcookie")]
        public async Task<IActionResult> SetCookie()
        {
            Response.Cookies.Append("c-a", "1111");
            Response.Cookies.Append("c-b", "2222");
            return Ok("ok");
        }

        [HttpGet("ignore")]
        public async Task<IActionResult> Ignore()
        {
            var message = await new HttpClient().GetStreamAsync("http://localhost:5002/api/values/ignore");
            return Ok(message);
        }

        [HttpGet("StopPropagation")]
        public async Task<IActionResult> StopPropagation()
        {
            var message = await new HttpClient().GetStreamAsync("http://localhost:5002/api/values/stoppropagation");
            return Ok(message);
        }

        [HttpGet("throw")]
        public async Task<IActionResult> Throw()
        {
            throw new NotImplementedException();
        }

        [HttpPost("logbody")]
        public async Task<IActionResult> LogBody([FromBody] Person person)
        {
            return Json(person);
        }

        [HttpPost("logformbody")]
        public async Task<IActionResult> LogFormBody([FromForm] Person person)
        {
            return Json(person);
        }

#if NETCOREAPP2_1
#else
        [HttpGet("greeter/grpc-net")]
        public async Task<IActionResult> GrpcNetAsync(string name)
        {
            const string Switch_AllowUnencryptedHttp2 = "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport";
            AppContext.SetSwitch(Switch_AllowUnencryptedHttp2, true);

            var channel = GrpcChannel.ForAddress("http://localhost:5003");
            var client = new Greeter.GreeterClient(channel);
            var result = await client.SayHelloAsync(new HelloRequest() { Name = name });
            return Ok(result);
        }
#endif
    }
}