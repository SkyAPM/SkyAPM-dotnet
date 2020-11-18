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

using System.Linq;
using System.Net.Http;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Filters;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.HttpClient.Handlers
{
    public class DefaultRequestDiagnosticHandler : IRequestDiagnosticHandler
    {
        private readonly HttpClientDiagnosticConfig _httpClientDiagnosticConfig;

        public DefaultRequestDiagnosticHandler(IConfigAccessor configAccessor)
        {
            _httpClientDiagnosticConfig = configAccessor.Get<HttpClientDiagnosticConfig>();
        }

        public bool OnlyMatch(HttpRequestMessage request)
        {
            return true;
        }

        public void Handle(ITracingContext tracingContext, HttpRequestMessage request)
        {
            var operationName = request.RequestUri.ToString();
            var shouldStopPropagation = _httpClientDiagnosticConfig.StopHeaderPropagationPaths != null 
                && _httpClientDiagnosticConfig.StopHeaderPropagationPaths
                    .Any(pattern => FastPathMatcher.Match(pattern, operationName));

            var context = tracingContext.CreateExitSegmentContext(operationName,
                $"{request.RequestUri.Host}:{request.RequestUri.Port}",
                shouldStopPropagation ? null : new HttpClientICarrierHeaderCollection(request));

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Common.Components.HTTPCLIENT;
            context.Span.AddTag(Tags.URL, request.RequestUri.ToString());
            context.Span.AddTag(Tags.HTTP_METHOD, request.Method.ToString());
        }
    }
}