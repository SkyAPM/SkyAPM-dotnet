using Grpc.Core;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public abstract class BaseGrpcClientDiagnosticProcessor
    {
        protected string GetOperationName(HttpRequestMessage request) => request.RequestUri.ToString();

        protected string GetHost(HttpRequestMessage request) => $"{request.RequestUri.Host}:{request.RequestUri.Port}";

        protected void InitializeCallSetupSpan(SegmentSpan span, HttpRequestMessage request)
        {
            span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            span.Component = Common.Components.GRPC;
            span.AddTag(Tags.URL, request.RequestUri.ToString());

            var activity = Activity.Current;
            if (activity.OperationName == GrpcDiagnostics.ActivityName)
            {
                var method = activity.Tags.FirstOrDefault(x => x.Key == GrpcDiagnostics.GrpcMethodTagName).Value ??
                             request.Method.ToString();

                span.AddTag(Tags.GRPC_METHOD_NAME, method);
            }
        }

        protected void FinishCallSetupSpan(TracingConfig tracingConfig, SegmentSpan span, HttpResponseMessage response)
        {
            var activity = Activity.Current;
            if (activity.OperationName == GrpcDiagnostics.ActivityName)
            {
                var statusCodeTag = activity.Tags.FirstOrDefault(x => x.Key == GrpcDiagnostics.GrpcStatusCodeTagName).Value;

                var statusCode = int.TryParse(statusCodeTag, out var code) ? code : -1;
                if (statusCode != 0)
                {
                    var err = ((StatusCode)statusCode).ToString();
                    span.ErrorOccurred(new Exception(err), tracingConfig);
                }

                span.AddTag(Tags.GRPC_STATUS, statusCode);
            }
        }
    }
}
