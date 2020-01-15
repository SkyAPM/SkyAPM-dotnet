/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public class GrpcHostingDiagnosticHandler : IHostingDiagnosticHandler
    {
        public const string ActivityName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";
        public const string GrpcMethodTagName = "grpc.method";
        public const string GrpcStatusCodeTagName = "grpc.status_code";

        public bool OnlyMatch(HttpContext httpContext)
        {
            return httpContext.Request.Headers.TryGetValue("Content-Type", out var value)
                   && value.Any(x => x == "application/grpc");
        }

        public void BeginRequest(ITracingContext tracingContext, HttpContext httpContext)
        {
            var context = tracingContext.CreateEntrySegmentContext(httpContext.Request.Path,
                new HttpRequestCarrierHeaderCollection(httpContext.Request));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Common.Components.GRPC;
            context.Span.Peer = new StringOrIntValue(httpContext.Connection.RemoteIpAddress.ToString());
            context.Span.AddTag(Tags.URL, httpContext.Request.GetDisplayUrl());
        }

        public void EndRequest(SegmentContext segmentContext, HttpContext httpContext)
        {
            var activity = Activity.Current;
            if (activity.OperationName == ActivityName)
            {
                var statusCodeTag = activity.Tags.FirstOrDefault(x => x.Key == GrpcStatusCodeTagName).Value;
                var method = activity.Tags.FirstOrDefault(x => x.Key == GrpcMethodTagName).Value ??
                             httpContext.Request.Method;

                segmentContext.Span.AddTag(Tags.GRPC_METHOD_NAME, method);

                var statusCode = int.TryParse(statusCodeTag, out var code) ? code : -1;
                if (statusCode != 0)
                {
                    segmentContext.Span.ErrorOccurred();
                }

                segmentContext.Span.AddTag(Tags.GRPC_STATUS, statusCode);
            }
        }
    }
}