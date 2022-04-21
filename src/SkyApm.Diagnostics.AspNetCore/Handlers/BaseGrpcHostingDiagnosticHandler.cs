using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Diagnostics;
using System.Linq;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public abstract class BaseGrpcHostingDiagnosticHandler
    {
        public const string ActivityName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";
        public const string GrpcMethodTagName = "grpc.method";
        public const string GrpcStatusCodeTagName = "grpc.status_code";

        protected bool IsMatch(HttpContext httpContext)
        {
            return httpContext.Request.Headers.TryGetValue("Content-Type", out var value) && value.Any(x => x == "application/grpc");
        }

        protected void BeginRequestSetupSpan(SegmentSpan span, HttpContext httpContext)
        {
            span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            span.Component = Components.GRPC;
            span.Peer = new StringOrIntValue(httpContext.Connection.RemoteIpAddress.ToString());
            span.AddTag(Tags.URL, httpContext.Request.GetDisplayUrl());
        }

        protected void EndRequestSetupSpan(SegmentSpan span, HttpContext httpContext)
        {
            var activity = Activity.Current;
            if (activity.OperationName == ActivityName)
            {
                var statusCodeTag = activity.Tags.FirstOrDefault(x => x.Key == GrpcStatusCodeTagName).Value;
                var method = activity.Tags.FirstOrDefault(x => x.Key == GrpcMethodTagName).Value ??
                             httpContext.Request.Method;

                span.AddTag(Tags.GRPC_METHOD_NAME, method);

                var statusCode = int.TryParse(statusCodeTag, out var code) ? code : -1;
                if (statusCode != 0)
                {
                    span.ErrorOccurred();
                }

                span.AddTag(Tags.GRPC_STATUS, statusCode);
            }
        }
    }
}
