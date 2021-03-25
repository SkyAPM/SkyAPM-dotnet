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
using Microsoft.AspNetCore.Http.Extensions;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http.Internal;
#endif
using Microsoft.Extensions.Primitives;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Common;
using SkyApm.Config;
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
    public class DefaultHostingDiagnosticHandler : IHostingDiagnosticHandler
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
            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Common.Components.ASPNETCORE;
            context.Span.Peer = new StringOrIntValue(httpContext.Connection.RemoteIpAddress.ToString());
            context.Span.AddTag(Tags.URL, httpContext.Request.GetDisplayUrl());
            context.Span.AddTag(Tags.PATH, httpContext.Request.Path);
            context.Span.AddTag(Tags.HTTP_METHOD, httpContext.Request.Method);

            if(_config.CollectCookies?.Count > 0)
            {
                var cookies = CollectCookies(httpContext, _config.CollectCookies);
                if (!string.IsNullOrEmpty(cookies))
                    context.Span.AddTag(Tags.HTTP_COOKIES, cookies);
            }

            if(_config.CollectHeaders?.Count > 0)
            {
                var headers = CollectHeaders(httpContext, _config.CollectHeaders);
                if (!string.IsNullOrEmpty(headers))
                    context.Span.AddTag(Tags.HTTP_HEADERS, headers);
            }

            if(_config.CollectBodyContentTypes?.Count > 0)
            {
                var body = CollectBody(httpContext, _config.CollectBodyLengthThreshold);
                if (!string.IsNullOrEmpty(body))
                    context.Span.AddTag(Tags.HTTP_REQUEST_BODY, body);
            }
        }

        public void EndRequest(SegmentContext segmentContext, HttpContext httpContext)
        {
            var statusCode = httpContext.Response.StatusCode;
            if (statusCode >= 400)
            {
                segmentContext.Span.ErrorOccurred();
            }

            segmentContext.Span.AddTag(Tags.STATUS_CODE, statusCode);
        }

        private string CollectCookies(HttpContext httpContext, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!httpContext.Request.Cookies.TryGetValue(key, out string value))
                    continue;

                if(sb.Length > 0)
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

                if(sb.Length > 0)
                    sb.Append('\n');

                sb.Append(key);
                sb.Append(": ");
                sb.Append(value);
            }
            return sb.ToString();
        }

        private string CollectBody(HttpContext httpContext, int lengthThreshold)
        {
            var request = httpContext.Request;

            if (string.IsNullOrEmpty(httpContext.Request.ContentType)
                || httpContext.Request.ContentLength == null
                || request.ContentLength > lengthThreshold)
            {
                return null;
            }

            var contentType = new ContentType(request.ContentType);
            if (!_config.CollectBodyContentTypes.Any(supportedType => contentType.MediaType == supportedType))
                return null;

#if NETSTANDARD2_0
            httpContext.Request.EnableRewind();
#else
            httpContext.Request.EnableBuffering();
#endif
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