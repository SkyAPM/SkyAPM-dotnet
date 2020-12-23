using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using SkyApm.Agent.AspNet;

namespace SkyApm.Sample.AspNet.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var httpClient = new HttpClient(new HttpTracingHandler());
            var values = await httpClient.GetStringAsync("http://localhost:5001/api/values");
            return Json(values);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<string> Get(int id)
        {
            var client = HttpClientFactory.Create(new HttpTracingHandler(null));
            Task.WhenAll(client.GetAsync("http://localhost:5002/api/delay/2000"),
                client.GetAsync("http://localhost:5002/api/values"),
                client.GetAsync("http://localhost:5002/api/delay/200"));
            return await client.GetStringAsync("http://localhost:5002/api/delay/100");
        }
    }
}