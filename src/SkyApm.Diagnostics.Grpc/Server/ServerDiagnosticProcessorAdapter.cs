using Grpc.Core;
using SkyApm.Config;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticProcessorAdapter : IServerDiagnosticProcessor
    {
        private readonly IServerDiagnosticProcessor _processor;

        public ServerDiagnosticProcessorAdapter(
            ServerDiagnosticProcessor defaultProcessor,
            SpanServerDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IServerDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            _processor.BeginRequest(grpcContext);
        }

        public void EndRequest(ServerCallContext grpcContext)
        {
            _processor.EndRequest(grpcContext);
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            _processor.DiagnosticUnhandledException(exception);
        }
    }
}
