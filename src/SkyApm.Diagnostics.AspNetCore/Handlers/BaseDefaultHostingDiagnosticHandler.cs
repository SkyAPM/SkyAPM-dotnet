using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using SkyApm.Common;
using SkyApm.Diagnostics.AspNetCore.Config;
using SkyApm.Diagnostics.AspNetCore.Extensions;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace SkyApm.Diagnostics.AspNetCore.Handlers
{
    public abstract class BaseDefaultHostingDiagnosticHandler
    {
        protected void BeginRequestSetupSpan(SegmentSpan span, HttpContext httpContext, HostingDiagnosticConfig config)
        {
            span.SpanLayer = SpanLayer.HTTP;
            span.Component = Common.Components.ASPNETCORE;
            span.Peer = new StringOrIntValue(httpContext.Connection.RemoteIpAddress.ToString());
            span.AddTag(Tags.URL, httpContext.Request.GetDisplayUrl());
            span.AddTag(Tags.PATH, httpContext.Request.Path);
            span.AddTag(Tags.HTTP_METHOD, httpContext.Request.Method);

            if (config.CollectCookies?.Count > 0)
            {
                var cookies = CollectCookies(httpContext, config.CollectCookies);
                if (!string.IsNullOrEmpty(cookies))
                    span.AddTag(Tags.HTTP_COOKIES, cookies);
            }

            if (config.CollectHeaders?.Count > 0)
            {
                var headers = CollectHeaders(httpContext, config.CollectHeaders);
                if (!string.IsNullOrEmpty(headers))
                    span.AddTag(Tags.HTTP_HEADERS, headers);
            }

            if (config.CollectBodyContentTypes?.Count > 0)
            {
                var body = CollectBody(httpContext, config.CollectBodyLengthThreshold, config);
                if (!string.IsNullOrEmpty(body))
                    span.AddTag(Tags.HTTP_REQUEST_BODY, body);
            }
        }

        protected void EndRequestSetupSpan(SegmentSpan span, HttpContext httpContext)
        {
            var statusCode = httpContext.Response.StatusCode;
            if (statusCode >= 400)
            {
                span.ErrorOccurred();
            }

            span.AddTag(Tags.STATUS_CODE, statusCode);
        }

        private string CollectCookies(HttpContext httpContext, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!httpContext.Request.Cookies.TryGetValue(key, out string value))
                    continue;

                if (sb.Length > 0)
                    sb.Append("; ");

                sb.Append(key);
                sb.Append('=');
                sb.Append(value);
            }
            return sb.ToString();
        }

        private string CollectHeaders(HttpContext httpContext, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!httpContext.Request.Headers.TryGetValue(key, out StringValues value))
                    continue;

                if (sb.Length > 0)
                    sb.Append('\n');

                sb.Append(key);
                sb.Append(": ");
                sb.Append(value);
            }
            return sb.ToString();
        }

        private string CollectBody(HttpContext httpContext, int lengthThreshold, HostingDiagnosticConfig config)
        {
            var request = httpContext.Request;

            if (string.IsNullOrEmpty(httpContext.Request.ContentType)
                || httpContext.Request.ContentLength == null
                || request.ContentLength > lengthThreshold)
            {
                return null;
            }

            var contentType = new ContentType(request.ContentType);
            if (!config.CollectBodyContentTypes.Any(supportedType => contentType.MediaType == supportedType))
                return null;

            httpContext.Request.EnableBuffering();
            request.Body.Position = 0;
            try
            {
                var encoding = contentType.CharSet.ToEncoding(Encoding.UTF8);
                using (var reader = new StreamReader(request.Body, encoding, true, 1024, true))
                {
                    var body = reader.ReadToEndAsync().Result;
                    return body;
                }
            }
            finally
            {
                request.Body.Position = 0;
            }
        }
    }
}
