using Grpc.Core;
using Grpc.Core.Interceptors;
using SkyApm.Config;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public class SpanClientDiagnosticProcessor : BaseClientDiagnosticProcessor, IClientDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanClientDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public Metadata BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext)
            where TRequest : class
            where TResponse : class
        {
            // 调用grpc方法时如果没有通过WithHost()方法指定grpc服务地址，则grpcContext.Host会为null，
            // 但context.Span.Peer为null的时候无法形成一条完整的链路，故设置了默认值[::1]
            var host = GetHost(grpcContext);
            var carrierHeader = new GrpcCarrierHeaderCollection(grpcContext.Options.Headers);
            var span = _tracingContext.CreateExitSpan($"{host}{grpcContext.Method.FullName}", host, carrierHeader);

            return BeginRequestSetupSpan(span, carrierHeader, host, grpcContext);
        }

        public void EndRequest()
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            EndRequestSetupSpan(span);

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
