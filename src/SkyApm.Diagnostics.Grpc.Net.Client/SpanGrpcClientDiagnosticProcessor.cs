using System.Net.Http;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public class SpanGrpcClientDiagnosticProcessor : BaseGrpcClientDiagnosticProcessor, IGrpcClientDiagnosticProcessor
    {
        public string ListenerName => GrpcDiagnostics.ListenerName;

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanGrpcClientDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStartKey)]
        public void InitializeCall([Property(Name = "Request")] HttpRequestMessage request)
        {
            var span = _tracingContext.CreateExitSpan(GetOperationName(request), GetHost(request), new GrpcNetClientICarrierHeaderCollection(request));

            InitializeCallSetupSpan(span, request);
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStopKey)]
        public void FinishCall([Property(Name = "Response")] HttpResponseMessage response)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            FinishCallSetupSpan(_tracingConfig, span, response);

            _tracingContext.StopSpan(span);
        }
    }
}
