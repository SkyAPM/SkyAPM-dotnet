using System;
using System.Net.Http;

namespace SkyApm.Diagnostics.HttpClient
{
    public interface IHttpClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        void HttpRequest(HttpRequestMessage request);

        void HttpResponse(HttpResponseMessage response);

        void HttpException(HttpRequestMessage request, Exception exception);
    }
}
