using Grpc.Core;
using Grpc.Core.Interceptors;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public abstract class BaseClientDiagnosticProcessor
    {
        protected string GetHost<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext) where TRequest : class where TResponse : class
        {
            return grpcContext.Host ?? "[::1]";
        }

        protected Metadata BeginRequestSetupSpan<TRequest, TResponse>(SegmentSpan span, GrpcCarrierHeaderCollection carrierHeader, string host, ClientInterceptorContext<TRequest, TResponse> grpcContext) where TRequest : class where TResponse : class
        {
            span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            span.Component = Components.GRPC;
            span.Peer = new StringOrIntValue(host);
            span.AddTag(Tags.GRPC_METHOD_NAME, grpcContext.Method.FullName);
            span.AddLog(
                LogEvent.Event("Grpc Client BeginRequest"),
                LogEvent.Message($"Request starting {grpcContext.Method.Type} {grpcContext.Method.FullName}"));

            var metadata = new Metadata();
            foreach (var item in carrierHeader)
            {
                metadata.Add(item.Key, item.Value);
            }
            return metadata;
        }

        protected void EndRequestSetupSpan(SegmentSpan span)
        {
            span.AddLog(
                LogEvent.Event("Grpc Client EndRequest"),
                LogEvent.Message($"Request finished "));
        }

        protected void DiagnosticUnhandledExceptionSetupSpan(TracingConfig tracingConfig, SegmentSpan span, Exception exception)
        {
            span?.ErrorOccurred(exception, tracingConfig);
        }
    }
}
