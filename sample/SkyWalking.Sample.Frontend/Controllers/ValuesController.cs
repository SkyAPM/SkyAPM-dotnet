using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace SkyWalking.Sample.Frontend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await new HttpClient().GetAsync("http://localhost:5002/api/values");
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("~/publish")]
        public IActionResult CapPublish([FromServices] ICapPublisher capPublisher)
        {
            using (var connection = new MySqlConnection(Startup.ConnectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                capPublisher.Publish("skywalking.cap.publish", new Person { Name = "Alexina", Age = 18 }, transaction);

                transaction.Commit();
            }
            return Ok();
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}