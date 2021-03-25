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
using System.Net.Http;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.HttpClient.Config;
using SkyApm.Diagnostics.HttpClient.Extensions;
using SkyApm.Diagnostics.HttpClient.Filters;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.HttpClient
{
    public class HttpClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "HttpHandlerDiagnosticListener";

        //private readonly IContextCarrierFactory _contextCarrierFactory;
        private readonly ITracingContext _tracingContext;

        private readonly IExitSegmentContextAccessor _contextAccessor;

        private readonly IEnumerable<IRequestDiagnosticHandler> _requestDiagnosticHandlers;

        private readonly TracingConfig _tracingConfig;

        private readonly HttpClientDiagnosticConfig _httpClientDiagnosticConfig;

        public HttpClientTracingDiagnosticProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor,
            IEnumerable<IRequestDiagnosticHandler> requestDiagnosticHandlers,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
            _requestDiagnosticHandlers = requestDiagnosticHandlers.Reverse();
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _httpClientDiagnosticConfig = configAccessor.Get<HttpClientDiagnosticConfig>();
        }

        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            foreach (var handler in _requestDiagnosticHandlers)
            {
                if (handler.OnlyMatch(request))
                {
                    handler.Handle(_tracingContext, request);
                    return;
                }
            }
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            var context = _contextAccessor.Context;
            if (context == null)
            {
                return;
            }

            if (response != null)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 400)
                {
                    context.Span.ErrorOccurred();
                }

                context.Span.AddTag(Tags.STATUS_CODE, statusCode);

                if(response.Content != null && _httpClientDiagnosticConfig.CollectResponseBodyContentTypes?.Count > 0)
                {
                    var responseBody = response.Content.TryCollectAsString(
                        _httpClientDiagnosticConfig.CollectResponseBodyContentTypes,
                        _httpClientDiagnosticConfig.CollectBodyLengthThreshold);
                    if (!string.IsNullOrEmpty(responseBody))
                        context.Span.AddTag(Tags.HTTP_RESPONSE_BODY, responseBody);
                }
            }

            _tracingContext.Release(context);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception)
        {
            _contextAccessor.Context?.Span?.ErrorOccurred(exception, _tracingConfig);
        }
    }
}