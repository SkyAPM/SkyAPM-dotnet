using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Sample.Backend.Controllers
{
    [ApiController]
    [Route("/test")]
    public class TestController : ControllerBase
    {
        private readonly ITracingContext _tracingContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TestController(ITracingContext tracingContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _tracingContext = tracingContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("")]
        public async Task<string> Get()
        {
            var outer = _tracingContext.CreateLocal("GetOuter");
            var inner = _tracingContext.CreateLocal("GetInner");
            await Delay("inner");
            _tracingContext.Finish(inner);
            _tracingContext.Finish(_tracingContext.CreateLocal("GetInner-2"));
            WrapDelay();
            _tracingContext.Finish(outer);

            var result = await PutAsync(1);
            WrapRequest();

            return $"get -> {result}";
        }

        [HttpGet]
        [Route("delay")]
        public async Task<int> Delay(int millisecond)
        {
            await Task.Delay(millisecond);
            return millisecond;
        }

        [HttpGet]
        [Route("multiReq")]
        public async Task<string> MultiRequest()
        {
            await PutAsync(1);
            await PostAsync(2);
            await PutAsync(3);
            await PutAsync(4);
            await PostAsync(5);
            await PutAsync(6);
            await PutAsync(7);
            await PutAsync(8);
            await PutAsync(9);

            return "123456789";
        }

        [HttpGet]
        [Route("asyncNoLocal")]
        public string AsyncNoLocal()
        {
            DelayAsync(6000);
            return "ok";
        }

        [HttpGet]
        [Route("asyncInAsync")]
        public string AsyncInAsync()
        {
            Async1();
            return "ok";
        }

        private async Task Async1()
        {
            var context = _tracingContext.CreateLocal("async1");
            await Task.Delay(6000);
            Async2();
            _tracingContext.Finish(context);
        }

        private async Task Async2()
        {
            var context = _tracingContext.CreateLocal("async2");
            await Task.Delay(10000);
            _tracingContext.Finish(context);
        }

        [HttpPost]
        [Route("")]
        public string Post([FromQuery] int flag)
        {
            Task.Run(() =>
            {
                var outer = _tracingContext.CreateLocal($"PostBackgroundTask-{flag}");
                Task.Run(() =>
                {
                    var inner = _tracingContext.CreateLocal($"PostBackgroundTaskInner-{flag}");
                    Thread.Sleep(2000);
                    _tracingContext.Finish(_tracingContext.CreateLocal($"PostBackgroundTaskInner-{flag}.Instantaneous"));
                    _tracingContext.Finish(inner);
                });
                Thread.Sleep(1000);
                _tracingContext.Finish(outer);
            });
            Thread.Sleep(500);
            return $"post-{flag}";
        }

        [HttpPut]
        [Route("")]
        public string Put([FromQuery] int flag)
        {
            _tracingContext.Finish(_tracingContext.CreateLocal($"Put-{flag}"));
            return $"put-{flag}";
        }

        [HttpGet]
        [Route("exception")]
        public string Exception()
        {
            throw new InvalidOperationException("invalid");
        }

        private async Task<string> WrapRequest()
        {
            var sos1 = _tracingContext.CreateLocal("WrapRequest-Outer");
            var sos2 = _tracingContext.CreateLocal("WrapRequest-Inner");
            _tracingContext.Finish(_tracingContext.CreateLocal("WrapRequest-1"));
            var result = await DelayAsync(2000);
            _tracingContext.Finish(_tracingContext.CreateLocal("WrapRequest-2"));
            _tracingContext.Finish(sos2);
            _tracingContext.Finish(sos1);
            return result;
        }

        private async Task<string> PostAsync(int flag)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync($"http://127.0.0.1:5001/test?flag={flag}", new StringContent(string.Empty));
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PutAsync(int flag)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PutAsync($"http://127.0.0.1:5001/test?flag={flag}", new StringContent(string.Empty));
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> DelayAsync(int millisecond)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:5001/test/delay?millisecond={millisecond}");
            return await response.Content.ReadAsStringAsync();
        }

        private async Task WrapDelay()
        {
            var wrapDelay = _tracingContext.CreateLocal("WrapDelay");
            await Task.Delay(3000);
            await Delay("wrap");
            _tracingContext.Finish(wrapDelay);
        }

        private async Task Delay(string suffix)
        {
            var disposable = _tracingContext.CreateLocal($"Delay.{suffix}");
            await Task.Delay(500);
            _tracingContext.Finish(disposable);
        }
    }
}
