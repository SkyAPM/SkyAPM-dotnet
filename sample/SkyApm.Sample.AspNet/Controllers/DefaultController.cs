using System;
using System.Web.Http;

namespace SkyApm.Sample.AspNet.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpGet]
        [Route("test")]
        public IHttpActionResult Test()
        {
            return Json(Environment.GetEnvironmentVariables());
        }
    }
}
