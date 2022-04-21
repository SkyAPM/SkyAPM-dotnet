using Grpc.Core;
using Grpc.Core.Interceptors;
using SkyApm.Config;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public class ClientDiagnosticProcessorAdapter : IClientDiagnosticProcessor
    {
        private readonly IClientDiagnosticProcessor _processor;

        public ClientDiagnosticProcessorAdapter(
            ClientDiagnosticProcessor defaultProcessor,
            SpanClientDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IClientDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public Metadata BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext)
            where TRequest : class
            where TResponse : class
        {
            return _processor.BeginRequest(grpcContext);
        }

        public void EndRequest()
        {
            _processor.EndRequest();
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            _processor.DiagnosticUnhandledException(exception);
        }
    }
}
