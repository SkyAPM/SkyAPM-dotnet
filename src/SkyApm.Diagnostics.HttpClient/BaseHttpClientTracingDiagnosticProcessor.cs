using SkyApm.Common;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Extensions;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Net.Http;

namespace SkyApm.Diagnostics.HttpClient
{
    public abstract class BaseHttpClientTracingDiagnosticProcessor
    {
        protected void HttpResponseSetupSpan(HttpClientDiagnosticConfig httpClientDiagnosticConfig, SegmentSpan span, HttpResponseMessage response)
        {
            if (response != null)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 400)
                {
                    span.ErrorOccurred();
                }

                span.AddTag(Tags.STATUS_CODE, statusCode);

                if (response.Content != null && httpClientDiagnosticConfig.CollectResponseBodyContentTypes?.Count > 0)
                {
                    var responseBody = response.Content.TryCollectAsString(
                        httpClientDiagnosticConfig.CollectResponseBodyContentTypes,
                        httpClientDiagnosticConfig.CollectBodyLengthThreshold);
                    if (!string.IsNullOrEmpty(responseBody))
                        span.AddTag(Tags.HTTP_RESPONSE_BODY, responseBody);
                }
            }
        }
    }
}
