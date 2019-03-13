using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SkyApm.Sample.AspNet.Controllers
{
    public class DefaultController : ApiController
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [HttpGet]
        [Route("test")]
        public IHttpActionResult Test()
        {
            return Json(Environment.GetEnvironmentVariables());
        }

        [HttpGet]
        [Route("test2")]
        public async Task<IHttpActionResult> Test2()
        {
            var values = await HttpClient.GetStringAsync("http://localhost:5001/api/values");
            return Json(values);
        }
    }
}
