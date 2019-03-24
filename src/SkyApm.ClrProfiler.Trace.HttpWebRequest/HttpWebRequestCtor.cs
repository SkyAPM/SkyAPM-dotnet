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
 using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.HttpWebRequest
{
    public class HttpWebRequestCtor: AbsMethodWrapper
    {
        private const string TypeName = "System.Net.HttpWebRequest";
        private const string AssemblyName = "System";
        private const string MethodName = ".ctor";

        private readonly ITracingContext _tracingContext;

        public HttpWebRequestCtor(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _tracingContext = (ITracingContext)serviceProvider.GetService(typeof(ITracingContext));
        }

        public override EndMethodDelegate BeforeWrappedMethod(TraceMethodInfo traceMethodInfo)
        {
            HttpWebRequestDiagnostic.Instance.Initialize(_tracingContext);
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

