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

using System.Net.Http;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public class GrpcClientDiagnosticProcessor : BaseGrpcClientDiagnosticProcessor, IGrpcClientDiagnosticProcessor
    {
        public string ListenerName => GrpcDiagnostics.ListenerName;

        //private readonly IContextCarrierFactory _contextCarrierFactory;
        private readonly ITracingContext _tracingContext;

        private readonly IExitSegmentContextAccessor _contextAccessor;
        private readonly TracingConfig _tracingConfig;

        public GrpcClientDiagnosticProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStartKey)]
        public void InitializeCall([Property(Name = "Request")] HttpRequestMessage request)
        {
            var context = _tracingContext.CreateExitSegmentContext(GetOperationName(request),
                GetHost(request),
                new GrpcNetClientICarrierHeaderCollection(request));

            InitializeCallSetupSpan(context.Span, request);
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStopKey)]
        public void FinishCall([Property(Name = "Response")] HttpResponseMessage response)
        {
            var context = _contextAccessor.Context;
            if (context == null)
            {
                return;
            }

            FinishCallSetupSpan(_tracingConfig, context.Span, response);

            _tracingContext.Release(context);
        }
    }
}