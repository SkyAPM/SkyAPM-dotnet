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
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.ClrProfiler.Trace.AspNetCore
{
    public class ProfilerMiddleWare
    {
        private const string SchemeDelimiter = "://";
        private readonly RequestDelegate _next;
        private readonly ITracingContext _tracingContext;

        public ProfilerMiddleWare(RequestDelegate next, ITracingContext tracer)
        {
            _tracingContext = tracer;
            _next = next;
        }

        private static string GetDisplayUrl(HttpRequest request)
        {
            var scheme = request.Scheme ?? string.Empty;
            var host = request.Host.Value ?? string.Empty;
            var pathBase = request.PathBase.Value ?? string.Empty;
            var path = request.Path.Value ?? string.Empty;
            var queryString = request.QueryString.Value ?? string.Empty;

            var length = scheme.Length + SchemeDelimiter.Length + host.Length
                         + pathBase.Length + path.Length + queryString.Length;

            return new StringBuilder(length)
                .Append(scheme)
                .Append(SchemeDelimiter)
                .Append(host)
                .Append(pathBase)
                .Append(path)
                .Append(queryString)
                .ToString();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var context = _tracingContext.CreateEntrySegmentContext(httpContext.Request.Path,
                new HttpRequestCarrierHeaderCollection(httpContext.Request));

            var displayUrl = GetDisplayUrl(httpContext.Request);

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Components.ASPNETCORE;
            context.Span.Peer = new StringOrIntValue(httpContext.Connection.RemoteIpAddress.ToString());
            context.Span.AddTag(Tags.URL, GetDisplayUrl(httpContext.Request));
            context.Span.AddTag(Tags.PATH, httpContext.Request.Path);
            context.Span.AddTag(Tags.HTTP_METHOD, httpContext.Request.Method);
            context.Span.AddLog(
                LogEvent.Event("AspNetCore Hosting BeginRequest"),
                LogEvent.Message(
                    $"Request starting {httpContext.Request.Protocol} {httpContext.Request.Method} {displayUrl}"));

            await Next(httpContext, context);
        }

        private async Task Next(HttpContext httpContext, SegmentContext context)
        {
            try
            {
                await _next(httpContext);

                var statusCode = httpContext.Response.StatusCode;
                if (statusCode >= 400)
                {
                    context.Span.ErrorOccurred();
                }

                context.Span.AddTag(Tags.STATUS_CODE, statusCode);
                context.Span.AddLog(
                    LogEvent.Event("AspNetCore Hosting EndRequest"),
                    LogEvent.Message(
                        $"Request finished {httpContext.Response.StatusCode} {httpContext.Response.ContentType}"));
            }
            catch (Exception ex)
            {
                context.Span.ErrorOccurred(ex);
            }

            _tracingContext.Release(context);
        }
    }
}

