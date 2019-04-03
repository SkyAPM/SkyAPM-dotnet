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
using System.Net.Http;
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.ClrProfiler.Trace.HttpClient
{
    public class SystemHttpClient : AbsMethodWrapper
    {
        private const string TypeName = "System.Net.Http.HttpClient";
        private const string AssemblyName = "System.Net.Http";

        private const string MethodName = "SendAsync";

        private readonly ITracingContext _tracingContext;

        public SystemHttpClient(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            var request = (HttpRequestMessage)traceMethodInfo.MethodArguments[0];

            var context = _tracingContext.CreateExitSegmentContext(request.RequestUri.ToString(),
              $"{request.RequestUri.Host}:{request.RequestUri.Port}",
              new HttpClientICarrierHeaderCollection(request));

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Common.Components.HTTPCLIENT;
            context.Span.AddTag(Tags.URL, request.RequestUri.ToString());
            context.Span.AddTag(Tags.HTTP_METHOD, request.Method.ToString());

            traceMethodInfo.TraceContext = context;

            return delegate(object returnValue, Exception ex)
            { 
                DelegateHelper.AsyncMethodEnd(Leave, traceMethodInfo, ex, returnValue);

                _tracingContext.ReleaseScope();
            };
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {
            var context = (SegmentContext)traceMethodInfo.TraceContext;
            if (ex != null)
            {
                context.Span.ErrorOccurred(ex);
            }

            var response = (HttpResponseMessage)ret;
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
                if (traceMethodInfo.MethodBase.Name == MethodName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

