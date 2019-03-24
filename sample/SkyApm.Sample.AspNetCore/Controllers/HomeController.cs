using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Dapper;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace Samples.WebApi.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly ILogger _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("test1")]
        public async Task<IActionResult> Test1()
        {
            var task = HttpClient.GetAsync("http://127.0.0.1:8787/home/Test2?k=1");
            var task2 = HttpClient.GetAsync("http://127.0.0.1:8787/home/Test2?k=2");
            var task3 = HttpClient.GetAsync("http://127.0.0.1:8787/home/Test2?k=3");
            _ = Task.Run(async () =>
            {
                await HttpClient.GetAsync("http://127.0.0.1:8787/home/Test2");
            });
            var str = await task;
            await task2;
            await task3;
            return Json(str);
        }

        [HttpGet]
        [Route("test6")]
        public IActionResult Test6()
        {
            return Json(Environment.GetEnvironmentVariables());
        }

        [HttpGet]
        [Route("test5")]
        public IActionResult Test5()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return Json(assemblies.ToList().Select(n => new { Name = n.FullName }));
        }

        [HttpGet]
        [Route("test2")]
        public async Task<IActionResult> Test2(string test)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                using (var connection = new SqlConnection($"Data Source=.;Initial Catalog=tempdb;Integrated Security=True"))
                {
                    await connection.OpenAsync();

                    await connection.ExecuteScalarAsync("SELECT 1");

                    await connection.ExecuteReaderAsync("SELECT 1");
                }

                sw.Stop();
                return Json(sw.ElapsedMilliseconds);
            }
            catch(Exception ex)
            {
                return Json(ex);
            }
        }

        [HttpGet]
        [Route("test3")]

        public async Task<IActionResult> Test3(string test)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (var connection = new MySqlConnection($"User ID=root;Password=123456;Host=127.0.0.1;Port=3306;Database=test;Pooling=true;Min Pool Size=0;Max Pool Size=100;SslMode=None"))
            {
                connection.Open();

                connection.ExecuteScalar("SELECT 1");

                await connection.ExecuteReaderAsync("SELECT 1");
            }

            sw.Stop();
            return Json(sw.ElapsedMilliseconds);
        }

        private static readonly Lazy<IDatabase> RedisDb = new Lazy<IDatabase>(ValueFactory);

        private static IDatabase ValueFactory()
        {
            var redis = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            return redis.GetDatabase(0);
        }

        [HttpGet]
        [Route("test4")]

        public async Task<IActionResult> Test4()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var prefix = "StackExchange.Redis.";

            await RedisDb.Value.StringSetAsync($"{prefix}INCR", "0");

            await RedisDb.Value.StringGetAsync($"{prefix}INCR");

            sw.Stop();
            return Json(sw.ElapsedMilliseconds);
        }
    }
}
