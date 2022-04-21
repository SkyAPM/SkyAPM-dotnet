using System.Net.Http;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Filters;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.HttpClient.Handlers
{
    public class SpanDefaultRequestDiagnosticHandler : BaseDefaultRequestDiagnosticHandler, ISpanRequestDiagnosticHandler
    {
        private readonly HttpClientDiagnosticConfig _httpClientDiagnosticConfig;

        public SpanDefaultRequestDiagnosticHandler(IConfigAccessor configAccessor)
        {
            _httpClientDiagnosticConfig = configAccessor.Get<HttpClientDiagnosticConfig>();
        }

        public void Handle(ITracingContext tracingContext, HttpRequestMessage request)
        {
            var operationName = GetOperationName(request);

            var ignored = IsIgnore(_httpClientDiagnosticConfig, operationName, request);
            if (ignored) return;

            var host = GetHost(request);
            var carrierHeaders = GetCarrierHeaders(_httpClientDiagnosticConfig, operationName, request);

            var span = tracingContext.CreateExitSpan(operationName, host, carrierHeaders);

            HandleSetupSpan(_httpClientDiagnosticConfig, span, request);
        }
    }
}
