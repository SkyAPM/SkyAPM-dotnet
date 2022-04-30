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

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Grpc.Core;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.Grpc.Net.Client
{
    public class GrpcClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => GrpcDiagnostics.ListenerName;

        //private readonly IContextCarrierFactory _contextCarrierFactory;
        private readonly ITracingContext _tracingContext;

        private readonly TracingConfig _tracingConfig;

        public GrpcClientDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStartKey)]
        public void InitializeCall([Property(Name = "Request")] HttpRequestMessage request)
        {
            var spanOrSegment = _tracingContext.CreateExit(request.RequestUri.ToString(),
                $"{request.RequestUri.Host}:{request.RequestUri.Port}",
                new GrpcNetClientICarrierHeaderCollection(request));

            spanOrSegment.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            spanOrSegment.Span.Component = Common.Components.GRPC;
            spanOrSegment.Span.AddTag(Tags.URL, request.RequestUri.ToString());

            var activity = Activity.Current;
            if (activity.OperationName == GrpcDiagnostics.ActivityName)
            {
                var method = activity.Tags.FirstOrDefault(x => x.Key == GrpcDiagnostics.GrpcMethodTagName).Value ??
                             request.Method.ToString();

                spanOrSegment.Span.AddTag(Tags.GRPC_METHOD_NAME, method);
            }
        }

        [DiagnosticName(GrpcDiagnostics.ActivityStopKey)]
        public void FinishCall([Property(Name = "Response")] HttpResponseMessage response)
        {
            var spanOrSegment = _tracingContext.CurrentExit;
            if (spanOrSegment == null)
            {
                return;
            }

            var activity = Activity.Current;
            if (activity.OperationName == GrpcDiagnostics.ActivityName)
            {
                var statusCodeTag = activity.Tags.FirstOrDefault(x => x.Key == GrpcDiagnostics.GrpcStatusCodeTagName).Value;

                var statusCode = int.TryParse(statusCodeTag, out var code) ? code : -1;
                if (statusCode != 0)
                {
                    var err = ((StatusCode)statusCode).ToString();
                    spanOrSegment.Span.ErrorOccurred(new Exception(err), _tracingConfig);
                }

                spanOrSegment.Span.AddTag(Tags.GRPC_STATUS, statusCode);
            }

            _tracingContext.Finish(spanOrSegment);
        }
    }
}