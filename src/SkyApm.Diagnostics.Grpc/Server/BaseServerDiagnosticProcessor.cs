using Grpc.Core;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public abstract class BaseServerDiagnosticProcessor
    {
        protected void BeginRequestSetupSpan(SegmentSpan span, ServerCallContext grpcContext)
        {
            span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            span.Component = Components.GRPC;
            span.Peer = new StringOrIntValue(grpcContext.Peer);
            span.AddTag(Tags.GRPC_METHOD_NAME, grpcContext.Method);
            span.AddLog(
                LogEvent.Event("Grpc Server BeginRequest"),
                LogEvent.Message($"Request starting {grpcContext.Method}"));
        }

        protected void EndRequestSetupSpan(SegmentSpan span, ServerCallContext grpcContext)
        {
            var statusCode = grpcContext.Status.StatusCode;
            if (statusCode != StatusCode.OK)
            {
                span.ErrorOccurred();
            }

            span.AddTag(Tags.GRPC_STATUS, statusCode.ToString());
            span.AddLog(
                LogEvent.Event("Grpc Server EndRequest"),
                LogEvent.Message($"Request finished {statusCode} "));
        }

        protected void DiagnosticUnhandledExceptionSetupSpan(TracingConfig tracingConfig, SegmentSpan span, Exception exception)
        {
            span?.ErrorOccurred(exception, tracingConfig);
        }
    }
}
