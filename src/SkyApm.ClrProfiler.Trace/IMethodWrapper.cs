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

namespace SkyApm.ClrProfiler.Trace
{
    internal interface IMethodWrapper
    {
        AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo);

        bool CanWrap(TraceMethodInfo traceMethodInfo);
    }

    public abstract class AbsMethodWrapper : IMethodWrapper
    {
        public virtual AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class NoopMethodWrapper : AbsMethodWrapper
    {
        public NoopMethodWrapper()
        {

        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            return null;
        }

        public override bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            return true;
        }
    }
}

