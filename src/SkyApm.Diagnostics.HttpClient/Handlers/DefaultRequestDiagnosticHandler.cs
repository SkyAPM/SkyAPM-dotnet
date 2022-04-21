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
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Filters;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.HttpClient.Handlers
{
    public class DefaultRequestDiagnosticHandler : BaseDefaultRequestDiagnosticHandler, IRequestDiagnosticHandler
    {
        private readonly HttpClientDiagnosticConfig _httpClientDiagnosticConfig;

        public DefaultRequestDiagnosticHandler(IConfigAccessor configAccessor)
        {
            _httpClientDiagnosticConfig = configAccessor.Get<HttpClientDiagnosticConfig>();
        }

        public void Handle(ITracingContext tracingContext, HttpRequestMessage request)
        {
            var operationName = GetOperationName(request);

            var ignored = IsIgnore(_httpClientDiagnosticConfig, operationName, request);
            if (ignored)
            {
                return;
            }

            var host = GetHost(request);
            var carrierHeaders = GetCarrierHeaders(_httpClientDiagnosticConfig, operationName, request);

            var context = tracingContext.CreateExitSegmentContext(operationName, host, carrierHeaders);

            HandleSetupSpan(_httpClientDiagnosticConfig, context.Span, request);
        }
    }
}