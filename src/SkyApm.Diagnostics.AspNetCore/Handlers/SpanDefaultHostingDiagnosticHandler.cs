using Microsoft.AspNetCore.Http;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Config;
using SkyApm.Diagnostics.AspNetCore.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public class SpanDefaultHostingDiagnosticHandler : BaseDefaultHostingDiagnosticHandler, ISpanHostingDiagnosticHandler
    {
        private readonly ITracingContext _tracingContext;
        private readonly HostingDiagnosticConfig _config;

        public SpanDefaultHostingDiagnosticHandler(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _config = configAccessor.Get<HostingDiagnosticConfig>();
        }

        public bool OnlyMatch(HttpContext httpContext) => true;

        public SegmentSpan BeginRequest(HttpContext httpContext)
        {
            var span = _tracingContext.CreateEntrySpan(httpContext.Request.Path, new HttpRequestCarrierHeaderCollection(httpContext.Request));
            BeginRequestSetupSpan(span, httpContext, _config);

            return span;
        }

        public void EndRequest(SegmentSpan span, HttpContext httpContext)
        {
            EndRequestSetupSpan(span, httpContext);
        }
    }
}
