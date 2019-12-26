using Grpc.Core;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _segmentContextAccessor;

        public ServerDiagnosticProcessor(IEntrySegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
            _segmentContextAccessor = segmentContextAccessor;
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            var context = _tracingContext.CreateEntrySegmentContext(grpcContext.Method, new GrpcCarrierHeaderCollection(grpcContext.RequestHeaders));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.GRPC; 
            context.Span.Peer = new StringOrIntValue(grpcContext.Peer);
            context.Span.AddTag(Tags.GRPC_METHOD_NAME, grpcContext.Method);
            context.Span.AddLog(
                LogEvent.Event("Grpc Server BeginRequest"),
                LogEvent.Message($"Request starting {grpcContext.Method}"));
        }

        public void EndRequest(ServerCallContext grpcContext)
        {
            var context = _segmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }
            var statusCode = grpcContext.Status.StatusCode;
            if (statusCode != StatusCode.OK)
            {
                context.Span.ErrorOccurred();
            }

            context.Span.AddTag(Tags.GRPC_STATUS, statusCode.ToString());
            context.Span.AddLog(
                LogEvent.Event("Grpc Server EndRequest"),
                LogEvent.Message($"Request finished {statusCode} "));

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
