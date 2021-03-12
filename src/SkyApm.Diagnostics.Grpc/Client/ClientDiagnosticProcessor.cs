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
using Grpc.Core.Interceptors;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public class ClientDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _segmentContextAccessor;
        private readonly TracingConfig _tracingConfig;

        public ClientDiagnosticProcessor(IExitSegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _segmentContextAccessor = segmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public Metadata BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext)
            where TRequest : class
            where TResponse : class
        {
            // 调用grpc方法时如果没有通过WithHost()方法指定grpc服务地址，则grpcContext.Host会为null，
            // 但context.Span.Peer为null的时候无法形成一条完整的链路，故设置了默认值[::1]
            var host = grpcContext.Host ?? "[::1]";
            var carrierHeader = new GrpcCarrierHeaderCollection(grpcContext.Options.Headers);
            var context = _tracingContext.CreateExitSegmentContext($"{host}{grpcContext.Method.FullName}", host, carrierHeader);
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
                context.Span?.ErrorOccurred(exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
    }
}