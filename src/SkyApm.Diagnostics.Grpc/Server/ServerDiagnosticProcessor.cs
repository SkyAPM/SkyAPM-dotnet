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
using SkyApm.Config;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticProcessor : BaseServerDiagnosticProcessor, IServerDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _segmentContextAccessor;
        private readonly TracingConfig _tracingConfig;

        public ServerDiagnosticProcessor(IEntrySegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _segmentContextAccessor = segmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            var context = _tracingContext.CreateEntrySegmentContext(grpcContext.Method, new GrpcCarrierHeaderCollection(grpcContext.RequestHeaders));
            BeginRequestSetupSpan(context.Span, grpcContext);
        }

        public void EndRequest(ServerCallContext grpcContext)
        {
            var context = _segmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }

            EndRequestSetupSpan(context.Span, grpcContext);

            _tracingContext.Release(context);
        }

        public void DiagnosticUnhandledException(Exception exception)
        {
            var context = _segmentContextAccessor.Context;
            if (context != null)
            {
                DiagnosticUnhandledExceptionSetupSpan(_tracingConfig, context.Span, exception);
                _tracingContext.Release(context);
            }
        }
    }
}
