using System;
using Microsoft.AspNetCore.Mvc;

namespace SkyApm.Sample.AspNetCore.Controllers
{
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {
            return new JsonResult(Environment.GetEnvironmentVariables());
        }
    }
}
