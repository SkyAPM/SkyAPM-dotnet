using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SkyApm.Tracing;
using System;
using System.Collections.Generic;
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

        public TestController(ITracingContext tracingContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _tracingContext = tracingContext;
            _httpClientFactory = httpClientFactory;
        }

        #region Basic
        [HttpGet]
        [Route("")]
        public async Task<string> Get(string flag)
        {
            await Step1();
            await Step2();

            return $"get-{flag}";

            async Task Step1()
            {
                var step1 = _tracingContext.CreateLocal("get-step1");

                _tracingContext.Finish(_tracingContext.CreateLocal("get-step11"));

                await HttpDelayAsync(100);

                _tracingContext.Finish(step1);
            }

            async Task Step2()
            {
                var step2 = _tracingContext.CreateLocal("get-step2");

                await HttpDelayAsync(150);

                _tracingContext.Finish(_tracingContext.CreateLocal("get-step22"));

                _tracingContext.Finish(step2);
            }
        }

        [HttpPost]
        [Route("")]
        public string Post([FromQuery] string flag)
        {
            _tracingContext.Finish(_tracingContext.CreateLocal($"Post-{flag}"));
            return $"post-{flag}";
        }

        [HttpGet]
        [Route("delay")]
        public async Task<int> Delay(int milliseconds)
        {
            await Task.Delay(milliseconds);
            return milliseconds;
        }

        [HttpGet]
        [Route("exception")]
        public string Exception()
        {
            throw new InvalidOperationException("invalid");
        }
        #endregion Basic

        [HttpGet]
        [Route("multiRequest")]
        public async Task<string> MultiRequest()
        {
            await HttpPostAsync(1);
            await HttpPostAsync(2);
            await HttpPostAsync(3);
            await HttpExceptionAsync();
            await HttpPostAsync(5);

            return "12345";
        }

        [HttpGet]
        [Route("batchAwait")]
        public async Task<string> BatchAwait()
        {
            var tasks = new List<Task>();
            tasks.Add(HttpPostAsync(1));
            tasks.Add(HttpDelayAsync(1500));
            tasks.Add(HttpExceptionAsync());
            tasks.Add(HttpPostAsync(2));
            tasks.Add(HttpDelayAsync(2500));

            await Task.WhenAll(tasks);

            return $"BatchAwait-{tasks.Count}";
        }

        [HttpGet]
        [Route("notAwaitedOnTime")]
        public async Task<string> NotAwaitedOnTime()
        {
            WrappedHttpDelayAsync(1000);

            await DelayAsync(nameof(NotAwaitedOnTime), 500);

            await WrapDelay(1000);

            return nameof(NotAwaitedOnTime);
        }

        [HttpGet]
        [Route("notAwaitedOutofTime")]
        public async Task<string> NotAwaitedOutofTime()
        {
            WrappedHttpDelayAsync(2000);

            await WrapDelay(100);

            await DelayAsync(string.Empty, 200);

            return nameof(NotAwaitedOutofTime);
        }

        [HttpGet]
        [Route("notAwaitedOutofMergeTime")]
        public async Task<string> NotAwaitedOutofMergeTime()
        {
            WrappedHttpDelayAsync(10000);

            await WrapDelay(700);

            await DelayAsync(string.Empty, 200);

            return nameof(NotAwaitedOutofMergeTime);
        }

        [HttpGet]
        [Route("notAwaitedNoWrapLocal")]
        public string NotAwaitedNoWrapLocal()
        {
            RichHttpDelayAsync(100);
            return nameof(NotAwaitedNoWrapLocal);
        }

        [HttpGet]
        [Route("notAwaitedNoWrapLocalOutOfMergeTime")]
        public string NotAwaitedNoWrapLocalOutOfMergeTime()
        {
            RichHttpDelayAsync(10000);
            return nameof(NotAwaitedNoWrapLocalOutOfMergeTime);
        }

        [HttpGet]
        [Route("deeplyNotAwaited")]
        public string DeeplyNotAwaited()
        {
            OuterAsync();
            return nameof(DeeplyNotAwaited);

            async Task OuterAsync()
            {
                var outer = _tracingContext.CreateLocal("outer");

                await DelayAsync(nameof(OuterAsync), 1000);
                InnerAsync();

                _tracingContext.Finish(outer);
            }

            async Task InnerAsync()
            {
                var inner = _tracingContext.CreateLocal("inner");

                await DelayAsync(nameof(InnerAsync), 10000);
                _tracingContext.Finish(_tracingContext.CreateLocal("inner-step1"));
                await HttpDelayAsync(100);

                _tracingContext.Finish(inner);
            }
        }

        [HttpGet]
        [Route("backgroundTask")]
        public string BackgroundTask()
        {
            Task.Run(() =>
            {
                var outer = _tracingContext.CreateLocal("BackgroundTask-outer");
                Task.Run(() =>
                {
                    var inner = _tracingContext.CreateLocal("BackgroundTask-inner");
                    Thread.Sleep(1000);
                    _tracingContext.Finish(_tracingContext.CreateLocal("BackgroundTask-Instantaneous"));
                    _tracingContext.Finish(inner);
                });
                Thread.Sleep(1000);
                _tracingContext.Finish(outer);
            });
            Thread.Sleep(500);
            return nameof(BackgroundTask);
        }

        [HttpGet]
        [Route("backgroundSkyApmTask")]
        public string BackgroundSkyApmTask()
        {
            SkyApmTask.Run(() =>
            {
                var outer = _tracingContext.CreateLocal($"BackgroundSkyApmTask-outer");
                SkyApmTask.Run(() =>
                {
                    var inner = _tracingContext.CreateLocal($"BackgroundSkyApmTask-inner");
                    Thread.Sleep(1000);
                    _tracingContext.Finish(_tracingContext.CreateLocal($"BackgroundSkyApmTask-.Instantaneous"));
                    _tracingContext.Finish(inner);
                });
                Thread.Sleep(1000);
                _tracingContext.Finish(outer);
            });
            Thread.Sleep(500);
            return nameof(BackgroundSkyApmTask);
        }

        #region Private
        private async Task WrapDelay(int milliseconds)
        {
            var preDelay = milliseconds / 3;
            var wrapDelay = _tracingContext.CreateLocal("WrapDelay");
            await Task.Delay(preDelay);
            await DelayAsync("wrap", milliseconds - preDelay);
            _tracingContext.Finish(wrapDelay);
        }

        private async Task DelayAsync(string suffix, int milliseconds)
        {
            var disposable = _tracingContext.CreateLocal($"Delay.{suffix}");
            await Task.Delay(milliseconds);
            _tracingContext.Finish(disposable);
        }
        private async Task WrappedHttpDelayAsync(int milliseconds)
        {
            var delayWrap = _tracingContext.CreateLocal("HttpDelayWrap");
            await RichHttpDelayAsync(milliseconds);
            _tracingContext.Finish(delayWrap);
        }

        private async Task RichHttpDelayAsync(int milliseconds)
        {
            _tracingContext.Finish(_tracingContext.CreateLocal("WrappedHttpDelayAsync-before"));
            await HttpDelayAsync(milliseconds);
            _tracingContext.Finish(_tracingContext.CreateLocal("WrappedHttpDelayAsync-after"));
        }
        #endregion Private

        #region Http
        private async Task<string> HttpGetAsync(int flag)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:5001/test?flag={flag}");
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> HttpPostAsync(int flag)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync($"http://127.0.0.1:5001/test?flag={flag}", new StringContent(string.Empty));
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> HttpExceptionAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:5001/test/exception");
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> HttpDelayAsync(int milliseconds)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:5001/test/delay?milliseconds={milliseconds}");
            return await response.Content.ReadAsStringAsync();
        }
        #endregion Http
    }
}
