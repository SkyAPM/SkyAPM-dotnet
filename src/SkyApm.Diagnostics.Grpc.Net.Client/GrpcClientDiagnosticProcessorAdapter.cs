using SkyApm.Config;
using System.Net.Http;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public class GrpcClientDiagnosticProcessorAdapter : IGrpcClientDiagnosticProcessor
    {
        private IGrpcClientDiagnosticProcessor _processor;

        public GrpcClientDiagnosticProcessorAdapter(
            GrpcClientDiagnosticProcessor defaultProcessor,
            SpanGrpcClientDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IGrpcClientDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => GrpcDiagnostics.ListenerName;

        [DiagnosticName(GrpcDiagnostics.ActivityStartKey)]
        public void InitializeCall([Property(Name = "Request")] HttpRequestMessage request)
        {
            _processor.InitializeCall(request);
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStopKey)]
        public void FinishCall([Property(Name = "Response")] HttpResponseMessage response)
        {
            _processor.FinishCall(response);
        }
    }
}
