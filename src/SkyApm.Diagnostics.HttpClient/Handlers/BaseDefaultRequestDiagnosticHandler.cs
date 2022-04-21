using SkyApm.Common;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Extensions;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace SkyApm.Diagnostics.HttpClient.Handlers
{
    public abstract class BaseDefaultRequestDiagnosticHandler
    {
        public bool OnlyMatch(HttpRequestMessage request)
        {
            return true;
        }

        protected string GetOperationName(HttpRequestMessage request) => request.RequestUri.GetLeftPart(UriPartial.Path);

        protected bool IsIgnore(HttpClientDiagnosticConfig httpClientDiagnosticConfig, string operationName, HttpRequestMessage request)
        {
            return httpClientDiagnosticConfig.IgnorePaths != null
                && httpClientDiagnosticConfig.IgnorePaths
                    .Any(pattern => FastPathMatcher.Match(pattern, operationName));
        }

        protected string GetHost(HttpRequestMessage request) => $"{request.RequestUri.Host}:{request.RequestUri.Port}";

        protected ICarrierHeaderCollection GetCarrierHeaders(HttpClientDiagnosticConfig httpClientDiagnosticConfig, string operationName, HttpRequestMessage request)
        {
            var shouldStopPropagation = httpClientDiagnosticConfig.StopHeaderPropagationPaths != null
                && httpClientDiagnosticConfig.StopHeaderPropagationPaths
                    .Any(pattern => FastPathMatcher.Match(pattern, operationName));

            return shouldStopPropagation ? (ICarrierHeaderCollection)null : new HttpClientICarrierHeaderCollection(request);
        }

        protected void HandleSetupSpan(HttpClientDiagnosticConfig httpClientDiagnosticConfig, SegmentSpan span, HttpRequestMessage request)
        {
            span.SpanLayer = SpanLayer.HTTP;
            span.Component = Common.Components.HTTPCLIENT;
            span.AddTag(Tags.URL, request.RequestUri.ToString());
            span.AddTag(Tags.HTTP_METHOD, request.Method.ToString());

            if (httpClientDiagnosticConfig.CollectRequestHeaders?.Count > 0)
            {
                var headers = CollectHeaders(request, httpClientDiagnosticConfig.CollectRequestHeaders);
                if (!string.IsNullOrEmpty(headers))
                    span.AddTag(Tags.HTTP_HEADERS, headers);
            }

            if (request.Content != null && httpClientDiagnosticConfig.CollectRequestBodyContentTypes?.Count > 0)
            {
                var requestBody = request.Content.TryCollectAsString(
                    httpClientDiagnosticConfig.CollectRequestBodyContentTypes,
                    httpClientDiagnosticConfig.CollectBodyLengthThreshold);
                if (!string.IsNullOrEmpty(requestBody))
                    span.AddTag(Tags.HTTP_REQUEST_BODY, requestBody);
            }
        }

        protected string CollectHeaders(HttpRequestMessage request, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!request.Headers.TryGetValues(key, out var values))
                    continue;

                if (sb.Length > 0)
                    sb.Append('\n');

                sb.Append(key);
                sb.Append(": ");

                var isFirstValue = true;
                foreach (var value in values)
                {
                    if (isFirstValue)
                        isFirstValue = false;
                    else
                        sb.Append(',');

                    sb.Append(value);
                }
            }
            return sb.ToString();
        }
    }
}
