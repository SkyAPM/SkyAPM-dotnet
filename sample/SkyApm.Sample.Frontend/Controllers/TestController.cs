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
            var disposableOuter = NewSegmentOrSpan("[Get] - Outer!");
            var disposableInner = NewSegmentOrSpan("GetInner");
            await Delay("inner");
            disposableInner.Dispose();
            NewSegmentOrSpan("GetInner-2").Dispose();
            WrapDelay();
            disposableOuter.Dispose();

            var result = await PutAsync(1);
            //PostAsync(2);

            return $"get -> {result}";
        }

        [HttpGet]
        [Route("multiReq")]
        public async Task<string> MultiRequest()
        {
            await PutAsync(1);
            await PutAsync(2);
            await PutAsync(3);
            await PutAsync(4);
            await PutAsync(5);
            await PutAsync(6);
            await PutAsync(7);
            await PutAsync(8);
            await PutAsync(9);

            return "123456789";
        }

        [HttpPost]
        [Route("")]
        public string Post([FromQuery] int flag)
        {
            Task.Run(() =>
            {
                var disposable = NewSegmentOrSpan($"PostBackgroundTask-{flag}");
                Task.Run(() =>
                {
                    var disposableInner = NewSegmentOrSpan($"PostBackgroundTaskInner-{flag}");
                    Thread.Sleep(5000);
                    NewSegmentOrSpan($"PostBackgroundTaskInner-{flag}.Instantaneous").Dispose();
                    disposableInner.Dispose();
                });
                Thread.Sleep(3000);
                disposable.Dispose();
            });
            Thread.Sleep(500);
            return $"post-{flag}";
        }

        [HttpPut]
        [Route("")]
        public string Put([FromQuery] int flag)
        {
            NewSegmentOrSpan($"Put-{flag}").Dispose();
            return $"put-{flag}";
        }

        [HttpGet]
        [Route("exception")]
        public string Exception()
        {
            throw new InvalidOperationException("invalid");
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

        private async Task WrapDelay()
        {
            var disposable = NewSegmentOrSpan("WrapDelay");
            await Task.Delay(3000);
            await Delay("wrap");
            disposable.Dispose();
        }

        private async Task Delay(string suffix)
        {
            var disposable = NewSegmentOrSpan($"Delay.{suffix}");
            await Task.Delay(500);
            disposable.Dispose();
        }

        private IDisposable NewSegmentOrSpan(string operationName)
        {
            if ("span".Equals(_configuration.GetValue("SkyWalking:StructType", "segment"), StringComparison.OrdinalIgnoreCase))
            {
                var span = _tracingContext.CreateLocalSpan(operationName);
                return new SkyContext(_tracingContext, span);
            }

            var segmentContext = _tracingContext.CreateLocalSegmentContext(operationName);
            return new SkyContext(_tracingContext, segmentContext);
        }
    }

    class SkyContext : IDisposable
    {
        private readonly SegmentSpan _span;
        private readonly SegmentContext _segmentContext;
        private readonly ITracingContext _tracingContext;

        public SkyContext(ITracingContext tracingContext, SegmentSpan span)
        {
            _span = span;
            _tracingContext = tracingContext;
        }

        public SkyContext(ITracingContext tracingContext, SegmentContext context)
        {
            _segmentContext = context;
            _tracingContext = tracingContext;
        }

        public void Dispose()
        {
            if (_span != null)
            {
                _tracingContext.StopSpan(_span);
            }
            else
            {
                _tracingContext.Release(_segmentContext);
            }
        }
    }
}
