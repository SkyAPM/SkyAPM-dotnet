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

using Microsoft.AspNetCore.Http;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Config;
using SkyApm.Diagnostics.AspNetCore.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public class DefaultHostingDiagnosticHandler : BaseDefaultHostingDiagnosticHandler, IHostingDiagnosticHandler
    {
        private readonly HostingDiagnosticConfig _config;

        public DefaultHostingDiagnosticHandler(IConfigAccessor configAccessor)
        {
            _config = configAccessor.Get<HostingDiagnosticConfig>();
        }

        public bool OnlyMatch(HttpContext request)
        {
            return true;
        }

        public void BeginRequest(ITracingContext tracingContext, HttpContext httpContext)
        {
            var context = tracingContext.CreateEntrySegmentContext(httpContext.Request.Path,
                new HttpRequestCarrierHeaderCollection(httpContext.Request));
            BeginRequestSetupSpan(context.Span, httpContext, _config);
        }

        public void EndRequest(SegmentContext segmentContext, HttpContext httpContext)
        {
            EndRequestSetupSpan(segmentContext.Span, httpContext);
        }
    }
}