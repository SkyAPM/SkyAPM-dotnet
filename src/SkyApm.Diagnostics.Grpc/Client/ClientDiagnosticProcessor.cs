using Grpc.Core;
using Grpc.Core.Interceptors;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public class ClientDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _segmentContextAccessor;

        public ClientDiagnosticProcessor(IExitSegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
            _segmentContextAccessor = segmentContextAccessor;
        }

        public Metadata BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext) 
            where TRequest : class
            where TResponse : class
        {
            var host = grpcContext.Host;
            var carrierHeader = new GrpcCarrierHeaderCollection(grpcContext.Options.Headers);
            var context = _tracingContext.CreateExitSegmentContext(grpcContext.Method.FullName, host, carrierHeader);
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.GRPC;
            context.Span.Peer = new StringOrIntValue(host);
            context.Span.AddTag(Tags.GRPC_METHOD_NAME, grpcContext.Method.FullName);
            context.Span.AddLog(
                LogEvent.Event("Grpc Client BeginRequest"),
                LogEvent.Message($"Request starting {grpcContext.Method.Type} {grpcContext.Method.FullName}"));

            var metadata = new Metadata();
            foreach (var item in carrierHeader)
            {
                metadata.Add(item.Key, item.Value);
            }
            return metadata;
        }

        public void EndRequest()
        {
            var context = _segmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }

            context.Span.AddLog(
                LogEvent.Event("Grpc Client EndRequest"),
                LogEvent.Message($"Request finished "));

            _tracingContext.Release(context);
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            var context = _segmentContextAccessor.Context;
            if (context != null)
            {
                context.Span?.ErrorOccurred(exception);
                _tracingContext.Release(context);
            }
        }
    }
}
