using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SkyApm.Sample.Backend.Services;

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
            return new string[] {"value1", "value2"};
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

            return Ok(content);
        }

        [HttpGet("greeter/exception")]
        public async Task<IActionResult> SayHelloWithExceptionAsync(string name)
        {
            var message = await _greeter.SayHelloWithExceptionAsync(name);
            return Ok(message);
        }
    }
}