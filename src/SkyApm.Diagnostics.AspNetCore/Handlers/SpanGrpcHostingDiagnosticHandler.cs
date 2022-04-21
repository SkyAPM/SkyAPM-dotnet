using Microsoft.AspNetCore.Http;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public class SpanGrpcHostingDiagnosticHandler : BaseGrpcHostingDiagnosticHandler, ISpanHostingDiagnosticHandler
    {
        private readonly ITracingContext _tracingContext;

        public SpanGrpcHostingDiagnosticHandler(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public bool OnlyMatch(HttpContext httpContext) => IsMatch(httpContext);

        public SegmentSpan BeginRequest(HttpContext httpContext)
        {
            var span = _tracingContext.CreateEntrySpan(httpContext.Request.Path, new HttpRequestCarrierHeaderCollection(httpContext.Request));
            BeginRequestSetupSpan(span, httpContext);

            return span;
        }

        public void EndRequest(SegmentSpan span, HttpContext httpContext)
        {
            EndRequestSetupSpan(span, httpContext);
        }
    }
}
