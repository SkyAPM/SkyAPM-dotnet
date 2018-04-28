using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace SkyWalking.Sample.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<string> Get(int id)
        {
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync("http://www.baidu.com");
        }

        [CapSubscribe("skywalking.cap.publish")]
        public void Consumer(Person person)
        {
            Console.WriteLine(person.ToString());
            Console.WriteLine("[skywalking.cap.publish] subscriber has been invoked!");
        }

    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"Person output, Name:{Name}, Age:{Age}";
        }
    }
}