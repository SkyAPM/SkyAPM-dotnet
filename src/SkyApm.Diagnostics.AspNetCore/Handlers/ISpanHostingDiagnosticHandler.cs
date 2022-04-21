using Microsoft.AspNetCore.Http;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public interface ISpanHostingDiagnosticHandler
    {
        bool OnlyMatch(HttpContext httpContext);

        SegmentSpan BeginRequest(HttpContext httpContext);

        void EndRequest(SegmentSpan span, HttpContext httpContext);
    }
}
