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
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.ClrProfiler.Trace.AspNet
{
    public class HttpContextFinishPipelineRequest : AbsMethodWrapper
    {
        private const string TypeName = "System.Web.HttpContext";
        private const string AssemblyName = "System.Web";
        private const string MethodName = "FinishPipelineRequest";

        private readonly ITracingContext _tracingContext;        

        public HttpContextFinishPipelineRequest(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            if (HttpRuntime.UsingIntegratedPipeline)
            {
                HttpContext httpContext = traceMethodInfo.InvocationTarget as HttpContext;
                if (httpContext != null)
                {
                    var context = httpContext.Items["SkyApm.ClrProfiler.Trace.AspNet.TraceScope"] as SegmentContext;
                    httpContext.Items.Remove("SkyApm.ClrProfiler.Trace.AspNet.TraceScope");
                    if (context != null)
                    {
                        var statusCode = httpContext.Response.StatusCode;
                        if (statusCode >= 400)
                        {
                            context.Span.ErrorOccurred();
                        }

                        var exception = httpContext.Error;
                        if (exception != null)
                        {
                            context.Span.ErrorOccurred(exception);
                        }

                        context.Span.AddLog(LogEvent.Event("AspNet EndRequest"),
                            LogEvent.Message(
                                $"Request finished {httpContext.Response.StatusCode} {httpContext.Response.ContentType}"));

                        _tracingContext.Release(context);
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

