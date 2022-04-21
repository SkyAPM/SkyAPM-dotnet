using SkyApm.Config;
using System;
using System.Net.Http;

namespace SkyApm.Diagnostics.HttpClient
{
    public class HttpClientTracingDiagnosticProcessorAdapter : IHttpClientTracingDiagnosticProcessor
    {
        private readonly IHttpClientTracingDiagnosticProcessor _processor;

        public HttpClientTracingDiagnosticProcessorAdapter(
            HttpClientTracingDiagnosticProcessor defaultProcessor,
            SpanHttpClientTracingDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IHttpClientTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => "HttpHandlerDiagnosticListener";

        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            _processor.HttpRequest(request);
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            _processor.HttpResponse(response);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request, [Property(Name = "Exception")] Exception exception)
        {
            _processor.HttpException(request, exception);
        }
    }
}
