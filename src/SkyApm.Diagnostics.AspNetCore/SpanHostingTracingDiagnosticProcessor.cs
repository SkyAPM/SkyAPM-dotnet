using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Diagnostics.AspNetCore.Handlers;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyApm.AspNetCore.Diagnostics
{
    public class SpanHostingTracingDiagnosticProcessor : IHostingTracingDiagnosticProcessor
    {
        private const string SPAN_KEY = "skywaling.span.entry.key";

        public string ListenerName => "Microsoft.AspNetCore";

        private readonly ITracingContext _tracingContext;
        private readonly IEnumerable<ISpanHostingDiagnosticHandler> _handlers;
        private readonly TracingConfig _tracingConfig;

        public SpanHostingTracingDiagnosticProcessor(
            ITracingContext tracingContext,
            IEnumerable<ISpanHostingDiagnosticHandler> diagnosticHandlers,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _handlers = diagnosticHandlers.Reverse();
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        /// <remarks>
        /// Variable name starts with an upper case, because it's used for parameter binding. In both ASP .NET Core 2.x and 3.x we get an object in which 
        /// HttpContext of the current request is available under the `HttpContext` property.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void BeginRequest([Property(Name = "HttpContext")] HttpContext httpContext)
        {
            foreach (var handler in _handlers)
            {
                if (handler.OnlyMatch(httpContext))
                {
                    var span = handler.BeginRequest(httpContext);
                    httpContext.Items.TryAdd(SPAN_KEY, span);
                    break;
                }
            }
        }

        /// <remarks>
        /// See remarks in <see cref="BeginRequest(HttpContext)"/>.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void EndRequest([Property(Name = "HttpContext")] HttpContext httpContext)
        {
            if (!httpContext.Items.TryGetValue(SPAN_KEY, out var item) || !(item is SegmentSpan span)) return;
            httpContext.Items.Remove(SPAN_KEY);

            foreach (var handler in _handlers)
            {
                if (handler.OnlyMatch(httpContext))
                {
                    handler.EndRequest(span, httpContext);
                    break;
                }
            }

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            if (!httpContext.Items.TryGetValue(SPAN_KEY, out var item) || !(item is SegmentSpan span)) return;
            httpContext.Items.Remove(SPAN_KEY);

            span.ErrorOccurred(exception, _tracingConfig);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            if (!httpContext.Items.TryGetValue(SPAN_KEY, out var item) || !(item is SegmentSpan span)) return;
            httpContext.Items.Remove(SPAN_KEY);

            span.ErrorOccurred(exception, _tracingConfig);
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
