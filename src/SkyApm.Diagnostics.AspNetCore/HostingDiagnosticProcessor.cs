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
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using SkyApm.Diagnostics;
using SkyApm.Diagnostics.AspNetCore.Handlers;
using SkyApm.Tracing;

namespace SkyApm.AspNetCore.Diagnostics
{
    public class HostingTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "Microsoft.AspNetCore";

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _segmentContextAccessor;
        private readonly IEnumerable<IHostingDiagnosticHandler> _diagnosticHandlers;

        public HostingTracingDiagnosticProcessor(IEntrySegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext, IEnumerable<IHostingDiagnosticHandler> diagnosticHandlers)
        {
            _tracingContext = tracingContext;
            _diagnosticHandlers = diagnosticHandlers.Reverse();
            _segmentContextAccessor = segmentContextAccessor;
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void BeginRequest([Property] HttpContext httpContext)
        {
            foreach (var handler in _diagnosticHandlers)
            {
                if (handler.OnlyMatch(httpContext))
                {
                    handler.BeginRequest(_tracingContext, httpContext);
                    return;
                }
            }
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void EndRequest([Property] HttpContext httpContext)
        {
            var context = _segmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }

            foreach (var handler in _diagnosticHandlers)
            {
                if (handler.OnlyMatch(httpContext))
                {
                    handler.EndRequest(context, httpContext);
                    break;
                }
            }

            _tracingContext.Release(context);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _segmentContextAccessor.Context?.Span?.ErrorOccurred(exception);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _segmentContextAccessor.Context?.Span?.ErrorOccurred(exception);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void BeforeAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void AfterAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }
    }
}