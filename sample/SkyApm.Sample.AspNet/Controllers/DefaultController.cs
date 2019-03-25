using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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

        [HttpGet]
        [Route("test3")]
        public async Task<IHttpActionResult> Test3()
        {
            await HttpClient.GetStringAsync("http://localhost:59909/test");

            var request = (HttpWebRequest)WebRequest.Create("http://localhost:59909/test");
            request.Method = "GET";

            using (var response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var values = await reader.ReadToEndAsync();
                        return Json(values);
                    }
                }
            }
            return Json("");
        }
    }
}
