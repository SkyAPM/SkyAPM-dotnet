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
 using System.Net;
 using SkyApm.Common;
 using SkyApm.Tracing;
 using SkyApm.Tracing.Segments;

 namespace SkyApm.ClrProfiler.Trace.HttpWebRequest
{
    public class HttpWebRequestWrapper : AbsMethodWrapper
    {
        private const string TypeName = "System.Net.HttpWebRequest";
        private const string AssemblyName = "System";
        private const string GetResponse = "GetResponse";

        private readonly ITracingContext _tracingContext;

        public HttpWebRequestWrapper(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            var request = traceMethodInfo.InvocationTarget as System.Net.HttpWebRequest;
            if (request == null)
            {
                return null;
            }

            var operationName = request.RequestUri.ToString();
            var networkAddress = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            var context = _tracingContext.CreateExitSegmentContext(operationName, networkAddress,
                new CarrierHeaderCollection(request.Headers));

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Components.HTTPCLIENT;
            context.Span.AddTag(Tags.URL, request.RequestUri.ToString());
            context.Span.AddTag(Tags.PATH, request.RequestUri.PathAndQuery);
            context.Span.AddTag(Tags.HTTP_METHOD, request.Method);

            traceMethodInfo.TraceContext = context;

            return delegate (object returnValue, Exception ex)
            {
                Leave(traceMethodInfo, returnValue, ex);
            };
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {
            var context = (SegmentContext)traceMethodInfo.TraceContext;
            if (ex != null)
            {
                context.Span.ErrorOccurred(ex);
            }

            var response = (HttpWebResponse)ret;
            if (response != null)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 400)
                {
                    context.Span.ErrorOccurred();
                }

                context.Span.AddTag(Tags.STATUS_CODE, statusCode);
            }

            _tracingContext.Release(context);
        }

        public override bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            var invocationTargetType = traceMethodInfo.Type;
            var assemblyName = invocationTargetType.Assembly.GetName().Name;
            if (assemblyName == AssemblyName && TypeName == invocationTargetType.FullName)
            {
                if (traceMethodInfo.MethodBase.Name == GetResponse)
                {
                    return true;
                }
            }

            return false;
        }

        private class CarrierHeaderCollection : ICarrierHeaderCollection
        {
            private readonly WebHeaderCollection _headers;

            public CarrierHeaderCollection(WebHeaderCollection headers)
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

