using System.Net.Http;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public interface IGrpcClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        void InitializeCall(HttpRequestMessage request);

        void FinishCall(HttpResponseMessage response);
    }
}
