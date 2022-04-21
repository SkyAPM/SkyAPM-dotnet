using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using SkyApm.Config;
using SkyApm.Diagnostics;
using System;

namespace SkyApm.AspNetCore.Diagnostics
{
    public class HostingTracingDiagnosticProcessorAdapter : IHostingTracingDiagnosticProcessor
    {
        private readonly IHostingTracingDiagnosticProcessor _processor;

        public HostingTracingDiagnosticProcessorAdapter(
            IConfigAccessor configAccessor,
            HostingTracingDiagnosticProcessor defaultProcessor,
            SpanHostingTracingDiagnosticProcessor spanProcessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IHostingTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => "Microsoft.AspNetCore";

        /// <remarks>
        /// Variable name starts with an upper case, because it's used for parameter binding. In both ASP .NET Core 2.x and 3.x we get an object in which 
        /// HttpContext of the current request is available under the `HttpContext` property.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void BeginRequest([Property(Name = "HttpContext")] HttpContext httpContext)
        {
            _processor.BeginRequest(httpContext);
        }

        /// <remarks>
        /// See remarks in <see cref="BeginRequest(HttpContext)"/>.
        /// </remarks>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void EndRequest([Property(Name = "HttpContext")] HttpContext httpContext)
        {
            _processor.EndRequest(httpContext);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _processor.DiagnosticUnhandledException(httpContext, exception);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _processor.HostingUnhandledException(httpContext, exception);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void BeforeAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
            _processor.BeforeAction(actionDescriptor, httpContext);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void AfterAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
            _processor.AfterAction(actionDescriptor, httpContext);
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
            _processor.RegisterMasterActivity();
        }
    }
}
