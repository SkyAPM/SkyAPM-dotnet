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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using SkyApm.Logging;

 namespace SkyApm.ClrProfiler.Trace
{
    internal class MethodTraceFinderService
    {
        private readonly ConcurrentDictionary<uint, FunctionInfoCache> _functionInfosCache =
            new ConcurrentDictionary<uint, FunctionInfoCache>();

        private readonly ILogger _logger;
        private readonly IEnumerable<IMethodWrapper> _methodWrappers;

        public MethodTraceFinderService(ILoggerFactory loggerFactory, IEnumerable<IMethodWrapper> methodWrappers)
        {
            _logger = loggerFactory.CreateLogger(typeof(MethodTraceFinderService));
            _methodWrappers = methodWrappers;
        }

        public MethodTrace GetMethodTrace(object type,
            object invocationTarget,
            object[] methodArguments,
            uint functionToken)
        {
            try
            {
                var traceMethodInfo = new TraceMethodInfo
                {
                    InvocationTarget = invocationTarget,
                    MethodArguments = methodArguments,
                    Type = (Type)type
                };

                var functionInfo = GetFunctionInfoFromCache(functionToken, traceMethodInfo);
                traceMethodInfo.MethodBase = functionInfo.MethodBase;

                if (!traceMethodInfo.MethodBase.IsStatic)
                {
                    if (invocationTarget == null)
                    {
                        throw new ArgumentException(nameof(invocationTarget));
                    }
                }

                if (functionInfo.MethodWrapper == null)
                {
                    PrepareMethodWrapper(functionInfo, traceMethodInfo);
                }

                var @delegate = functionInfo.MethodWrapper?.BeginWrapMethod(traceMethodInfo);
                return @delegate == null ? null : new MethodTrace(@delegate);
            }
            catch (Exception ex)
            {
                _logger.Error("Get MethodWrapper Error", ex);
                return null;
            }
        }

        /// <summary>
        /// Prepare FunctionInfoCache MethodWrapperInfo
        /// </summary>
        /// <param name="functionInfo"></param>
        /// <param name="traceMethodInfo"></param>
        private void PrepareMethodWrapper(FunctionInfoCache functionInfo, TraceMethodInfo traceMethodInfo)
        {
            try
            {
                foreach (var methodWrapper in _methodWrappers)
                {
                    if (methodWrapper.CanWrap(traceMethodInfo))
                    {
                        functionInfo.MethodWrapper = methodWrapper;
                        break;
                    }
                }
                if (functionInfo.MethodWrapper == null)
                {
                    functionInfo.MethodWrapper = new NoopMethodWrapper();
                }
            }
            catch
            {
                functionInfo.MethodWrapper = new NoopMethodWrapper();
            }
        }

        /// <summary>
        /// GetFunctionInfo MethodBase FromCache
        /// </summary>
        /// <param name="functionToken"></param>
        /// <param name="traceMethodInfo"></param>
        /// <returns></returns>
        private FunctionInfoCache GetFunctionInfoFromCache(uint functionToken, TraceMethodInfo traceMethodInfo)
        {
            var functionInfo = _functionInfosCache.GetOrAdd(functionToken, token =>
            {
                var methodBase = traceMethodInfo.Type.Module.ResolveMethod((int) token);
                var functionInfoCache = new FunctionInfoCache
                {
                    MethodBase = methodBase
                };
                return functionInfoCache;
            });
            return functionInfo;
        }

        private class FunctionInfoCache
        {
            public IMethodWrapper MethodWrapper { get; set; }

            public MethodBase MethodBase { get; set; }
        }
    }
}

