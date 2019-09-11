using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkyApm.Sample.Backend.Services;

namespace SkyApm.Sample.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrpcController : ControllerBase
    {
        private readonly GreeterGrpcService _greeter;

        public GrpcController(GreeterGrpcService greeter)
        {
            _greeter = greeter;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new List<string> { "value1", "value2" };
        }

        [HttpGet("greeter")]
        public async Task<IActionResult> SayHelloAsync(string name)
        {
            var reply = await _greeter.SayHelloAsync(name);
            return Ok($"from backend grpc message{reply}");
        }
    }
}