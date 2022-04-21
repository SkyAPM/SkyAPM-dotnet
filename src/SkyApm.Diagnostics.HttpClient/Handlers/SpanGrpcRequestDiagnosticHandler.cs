using SkyApm.Diagnostics.HttpClient.Filters;

namespace SkyApm.Diagnostics.HttpClient.Handlers
{
    internal class SpanGrpcRequestDiagnosticHandler : GrpcRequestDiagnosticHandler, ISpanRequestDiagnosticHandler
    {
    }
}
