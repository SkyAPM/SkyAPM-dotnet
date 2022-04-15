using Microsoft.AspNetCore.Mvc;
using SkyApm.Tracing;

namespace Sky.Apm.Sample.Logging.Controllers
{
    [ApiController]
    [Route("apitest")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly Test _test;
        private readonly ILogger<WeatherForecastController> _logger;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, Test test)
        {
            _logger = logger;
            _test = test;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            //Console.WriteLine(_entrySegmentContextAccessor.Context?.TraceId);
            _logger.LogInformation("我用来测试自定义日志！！！！！！！！！！！！");
            _test.Create();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}