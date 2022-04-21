using Grpc.Core;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class SpanServerDiagnosticProcessor : BaseServerDiagnosticProcessor, IServerDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanServerDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            var span = _tracingContext.CreateEntrySpan(grpcContext.Method, new GrpcCarrierHeaderCollection(grpcContext.RequestHeaders));
            BeginRequestSetupSpan(span, grpcContext);
        }

        public void EndRequest(ServerCallContext grpcContext)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            EndRequestSetupSpan(span, grpcContext);

            _tracingContext.StopSpan(span);
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            DiagnosticUnhandledExceptionSetupSpan(_tracingConfig, span, exception);

            _tracingContext.StopSpan(span);
        }
    }
}
