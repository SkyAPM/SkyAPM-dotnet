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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.AspNetCore
{
    public class WebHostBuilder : AbsMethodWrapper
    {
        private const string TypeName = "Microsoft.AspNetCore.Hosting.WebHostBuilder";
        private const string AssemblyName = "Microsoft.AspNetCore.Hosting";
        private const string MethodName = "BuildCommonServices";

        private readonly ITracingContext _tracingContext;

        public WebHostBuilder(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            return delegate (object returnValue, Exception ex)
            {
                Leave(traceMethodInfo, returnValue, ex);
            };
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {
            var serviceCollection = (ServiceCollection) ret;
            serviceCollection.AddSingleton<IStartupFilter>(n => new ProfilerStartupFilter(_tracingContext));
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

