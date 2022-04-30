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

using Grpc.Core;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public ServerDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            var spanOrSegment = _tracingContext.CreateEntry(grpcContext.Method, new GrpcCarrierHeaderCollection(grpcContext.RequestHeaders));
            spanOrSegment.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            spanOrSegment.Span.Component = Components.GRPC; 
            spanOrSegment.Span.Peer = new StringOrIntValue(grpcContext.Peer);
            spanOrSegment.Span.AddTag(Tags.GRPC_METHOD_NAME, grpcContext.Method);
            spanOrSegment.Span.AddLog(
                LogEvent.Event("Grpc Server BeginRequest"),
                LogEvent.Message($"Request starting {grpcContext.Method}"));
        }

        public void EndRequest(ServerCallContext grpcContext)
        {
            var context = _tracingContext.CurrentEntry;
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

            _tracingContext.Finish(context);
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            var spanOrSegment = _tracingContext.CurrentEntry;
            if (spanOrSegment != null)
            {
                spanOrSegment.Span?.ErrorOccurred(exception, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
            }
        }
    }
}
