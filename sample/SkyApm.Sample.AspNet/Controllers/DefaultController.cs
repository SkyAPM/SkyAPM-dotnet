using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using StackExchange.Redis;

namespace SkyApm.Sample.AspNet.Controllers
{
    public class DefaultController : ApiController
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [HttpGet]
        [Route("home/test")]
        public IHttpActionResult Test()
        {
            return Json(Environment.GetEnvironmentVariables());
        }

        [HttpGet]
        [Route("home/test2")]
        public async Task<IHttpActionResult> Test2()
        {
            var values = await HttpClient.GetStringAsync("http://localhost:59909/home/test");
            return Json(values);
        }

        [HttpGet]
        [Route("home/test3")]
        public async Task<IHttpActionResult> Test3()
        {
            await HttpClient.GetStringAsync("http://localhost:59909/home/test");

            var request = (HttpWebRequest)WebRequest.Create("http://localhost:59909/home/test");
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

        private static readonly Lazy<IDatabase> RedisDb = new Lazy<IDatabase>(ValueFactory);

        private static IDatabase ValueFactory()
        {
            var redis = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            return redis.GetDatabase(0);
        }

        [HttpGet]
        [Route("home/test4")]

        public async Task<IHttpActionResult> Test4()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var prefix = "StackExchange.Redis.";

            await RedisDb.Value.StringSetAsync($"{prefix}INCR", "0");

            await HttpClient.GetAsync("http://localhost:59909/test");

            await RedisDb.Value.StringGetAsync($"{prefix}INCR");

            sw.Stop();
            return Json(sw.ElapsedMilliseconds);
        }
    }
}
