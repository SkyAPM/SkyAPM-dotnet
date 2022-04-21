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
        private readonly SkyApm.Logging.ILogger _loggerFactory;
        public WeatherForecastController(SkyApm.Logging.ILoggerFactory loggerFactory, Test test)
        {
            _loggerFactory = loggerFactory.CreateSkyApmLogger(GetType());
            _test = test;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            //Console.WriteLine(_entrySegmentContextAccessor.Context?.TraceId);
            _loggerFactory.Information("qiwnkasdnaskndlaknd laksndk asndkas ndl nsald nlasdn la£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡");
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