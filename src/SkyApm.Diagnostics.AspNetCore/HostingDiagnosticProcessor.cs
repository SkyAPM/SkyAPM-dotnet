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
using SkyApm.Config;
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
        private readonly TracingConfig _tracingConfig;

        public HostingTracingDiagnosticProcessor(IEntrySegmentContextAccessor segmentContextAccessor,
            ITracingContext tracingContext, IEnumerable<IHostingDiagnosticHandler> diagnosticHandlers,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _diagnosticHandlers = diagnosticHandlers.Reverse();
            _segmentContextAccessor = segmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        /// <remarks>
        /// Variable name starts with an upper case, because it's used for parameter binding. In both ASP .NET Core 2.x and 3.x we get an object in which 
        /// HttpContext of the current request is available under the `HttpContext` property.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void BeginRequest([Property] HttpContext HttpContext)
        {
            foreach (var handler in _diagnosticHandlers)
            {
                if (handler.OnlyMatch(HttpContext))
                {
                    handler.BeginRequest(_tracingContext, HttpContext);
                    return;
                }
            }
        }

        /// <remarks>
        /// See remarks in <see cref="BeginRequest(HttpContext)"/>.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void EndRequest([Property] HttpContext HttpContext)
        {
            var context = _segmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }

            foreach (var handler in _diagnosticHandlers)
            {
                if (handler.OnlyMatch(HttpContext))
                {
                    handler.EndRequest(context, HttpContext);
                    break;
                }
            }

            _tracingContext.Release(context);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _segmentContextAccessor.Context?.Span?.ErrorOccurred(exception, _tracingConfig);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _segmentContextAccessor.Context?.Span?.ErrorOccurred(exception, _tracingConfig);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void BeforeAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void AfterAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }

        /// <summary>
        /// Empty method used to register additional activity in diagnostic listener. This method normally will not be called.
        /// </summary>
        /// <remarks>
        /// Both `Microsoft.AspNetCore.Hosting.HttpRequestIn.Start` and `Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop` activities will be called only, 
        /// if diagnostic listener will have the `Microsoft.AspNetCore.Hosting.HttpRequestIn` activity enabled. Currently this is the only way to register an
        /// extra activity to be tracked, without extra code changes in SkyApm.Diagnostics.Tracing* classes.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn")]
        public void RegisterMasterActivity()
        {
        }
    }
}