using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System;

namespace SkyApm.AspNetCore.Diagnostics
{
    public interface IHostingTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        void BeginRequest(HttpContext HttpContext);

        void EndRequest(HttpContext HttpContext);

        void DiagnosticUnhandledException(HttpContext httpContext, Exception exception);

        void HostingUnhandledException(HttpContext httpContext, Exception exception);

        void BeforeAction(ActionDescriptor actionDescriptor, HttpContext httpContext);

        void AfterAction(ActionDescriptor actionDescriptor, HttpContext httpContext);

        void RegisterMasterActivity();
    }
}
