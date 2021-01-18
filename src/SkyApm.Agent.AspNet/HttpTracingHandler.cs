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
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Agent.AspNet
{
    public class HttpTracingHandler : DelegatingHandler
    {
        /// <summary>
        /// If you are passing this delegating handler to <see cref="HttpClientFactory.Create"/> method, please pass a `null` in constructor as innerHandler.
        /// if you are passing it in <see cref="HttpClient"/> constructor, just use it's non-parameter constructor.
        /// as follow:
        /// <code>var httpClient = HttpClientFactory.Create(new HttpTracingHandler(null))</code>
        /// or
        /// <code>var httpClient = new HttpClient(new HttpTracingHandler())</code>
        /// </summary>
        public HttpTracingHandler()
            : this(new HttpClientHandler())
        {
        }

        /// <summary>
        /// If you are passing this delegating handler to <see cref="HttpClientFactory.Create"/> method, please pass a `null` in constructor as innerHandler.
        /// if you are passing it in <see cref="HttpClient"/> constructor, just use it's non-parameter constructor.
        /// as follow:
        /// <code>var httpClient = HttpClientFactory.Create(new HttpTracingHandler(null))</code>
        /// or
        /// <code>var httpClient = new HttpClient(new HttpTracingHandler())</code>
        /// </summary>
        public HttpTracingHandler(HttpMessageHandler innerHandler)
        {
            if (innerHandler != null)
            {
                InnerHandler = innerHandler;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var tracingContext = ServiceLocator.Current.GetInstance<ITracingContext>();
            var operationName = request.RequestUri.ToString();
            var networkAddress = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            var context = tracingContext.CreateExitSegmentContext(operationName, networkAddress,
                new CarrierHeaderCollection(request.Headers));
            try
            {
                HttpResponseMessage response;

                context.Span.SpanLayer = SpanLayer.HTTP;
                context.Span.Component = Common.Components.HTTPCLIENT;
                context.Span.AddTag(Common.Tags.URL, request.RequestUri.ToString());
                context.Span.AddTag(Common.Tags.PATH, request.RequestUri.PathAndQuery);
                context.Span.AddTag(Common.Tags.HTTP_METHOD, request.Method.ToString());

                // With ConfigureAwait on next await call, current HttpContext will not be available for the rest of the method.
                // Because HttpContext is needed when we are using tracing context accessors, it must be stored in local variable.
                var currentHttpContext = System.Web.HttpContext.Current;

                // Prevents deadlock for synchronous calls to HttpClient methods. 
                // More here: C#: Why you should use ConfigureAwait(false) in your library code
                // https://medium.com/bynder-tech/c-why-you-should-use-configureawait-false-in-your-library-code-d7837dce3d7f
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Restoring HttpContext.
                System.Web.HttpContext.Current = currentHttpContext;

                var statusCode = (int)response.StatusCode;
                if (statusCode >= 400)
                {
                    context.Span.ErrorOccurred();
                }

                context.Span.AddTag(Common.Tags.STATUS_CODE, statusCode);
                return response;
            }
            catch (Exception exception)
            {
                context.Span.ErrorOccurred(exception);
                throw;
            }
            finally
            {
                tracingContext.Release(context);
            }
        }

        private class CarrierHeaderCollection : ICarrierHeaderCollection
        {
            private readonly HttpRequestHeaders _headers;

            public CarrierHeaderCollection(HttpRequestHeaders headers)
            {
                _headers = headers;
            }

            public void Add(string key, string value)
            {
                _headers.Add(key, value);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}