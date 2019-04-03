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
using System.Web;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.ClrProfiler.Trace.AspNet
{
    public class HttpApplicationExecuteStep : AbsMethodWrapper
    {
        private const string TypeName = "System.Web.HttpApplication";
        private const string AssemblyName = "System.Web";
        private const string MethodName = "ExecuteStep";

        private readonly ITracingContext _tracingContext;

        public HttpApplicationExecuteStep(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            if (HttpRuntime.UsingIntegratedPipeline)
            {
                HttpContext httpContext = (traceMethodInfo.InvocationTarget as HttpApplication)?.Context;
                if (httpContext != null)
                {
                    if (httpContext.Items["SkyApm.ClrProfiler.Trace.AspNet.TraceScope"] == null)
                    {
                        if (httpContext.Request.HttpMethod == "OPTIONS")
                        {
                            //asp.net Exclude OPTIONS request
                            return null;
                        }

                        var context = _tracingContext.CreateEntrySegmentContext(httpContext.Request.Path,
                            new HttpRequestCarrierHeaderCollection(httpContext.Request));

                        context.Span.SpanLayer = SpanLayer.HTTP;
                        context.Span.Peer = new StringOrIntValue(httpContext.Request.UserHostAddress);
                        context.Span.Component = Common.Components.ASPNET;
                        context.Span.AddTag(Tags.URL, httpContext.Request.Url.OriginalString);
                        context.Span.AddTag(Tags.PATH, httpContext.Request.Path);
                        context.Span.AddTag(Tags.HTTP_METHOD, httpContext.Request.HttpMethod);
                        context.Span.AddLog(LogEvent.Event("AspNet BeginRequest"),
                            LogEvent.Message(
                                $"Request starting {httpContext.Request.Url.Scheme} {httpContext.Request.HttpMethod} {httpContext.Request.Url.OriginalString}"));

                        httpContext.Items["SkyApm.ClrProfiler.Trace.AspNet.TraceScope"] = context;
                    }
                }
            }
            return null;
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

