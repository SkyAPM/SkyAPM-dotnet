using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Filters;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.HttpClient
{
    public class SpanHttpClientTracingDiagnosticProcessor : BaseHttpClientTracingDiagnosticProcessor, IHttpClientTracingDiagnosticProcessor
    {
        public string ListenerName => "HttpHandlerDiagnosticListener";

        private readonly ITracingContext _tracingContext;
        private readonly IEnumerable<ISpanRequestDiagnosticHandler> _handlers;
        private readonly TracingConfig _tracingConfig;
        private readonly HttpClientDiagnosticConfig _httpClientDiagnosticConfig;

        public SpanHttpClientTracingDiagnosticProcessor(
            ITracingContext tracingContext,
            IEnumerable<ISpanRequestDiagnosticHandler> handlers,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _handlers = handlers.Reverse();
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _httpClientDiagnosticConfig = configAccessor.Get<HttpClientDiagnosticConfig>();
        }

        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            foreach (var handler in _handlers)
            {
                if (handler.OnlyMatch(request))
                {
                    handler.Handle(_tracingContext, request);
                    return;
                }
            }
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            HttpResponseSetupSpan(_httpClientDiagnosticConfig, span, response);

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(exception, _tracingConfig);
        }
    }
}
