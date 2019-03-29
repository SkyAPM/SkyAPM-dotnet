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
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SkyApm.ClrProfiler.Trace
{
    public class MethodFinderService
    {
        private readonly ConcurrentDictionary<uint, FunctionInfoCache> _functionInfosCache =
            new ConcurrentDictionary<uint, FunctionInfoCache>();

        private readonly ConcurrentDictionary<string, AssemblyInfoCache> _assemblies = 
            new ConcurrentDictionary<string, AssemblyInfoCache>();

        private readonly IServiceProvider _serviceProvider;
        private readonly TraceEnvironment _traceEnvironment;

        public MethodFinderService(IServiceProvider serviceProvider,
            TraceEnvironment traceEnvironment)
        {
            _serviceProvider = serviceProvider;
            _traceEnvironment = traceEnvironment;

            InitAssemblyConfig();
        }

        private void InitAssemblyConfig()
        {
            try
            {
                var profilerHome = _traceEnvironment.GetProfilerHome();
                if (string.IsNullOrEmpty(profilerHome))
                {
                    throw new ArgumentException("CLR PROFILER HOME IsNullOrEmpty");
                }

                var path = Path.Combine(profilerHome, "trace.json");
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    var jObject = (JObject)JsonConvert.DeserializeObject(text);
                    foreach (var jToken in jObject["instrumentation"])
                    {
                        _assemblies.TryAdd(jToken["assemblyName"].ToString(), new AssemblyInfoCache
                        {
                            AssemblyName = jToken["targetAssemblyName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        public EndMethodDelegate BeforeWrappedMethod(object type,
            object invocationTarget,
            object[] methodArguments,
            uint functionToken)
        {      
            var traceMethodInfo = new TraceMethodInfo
            {
                InvocationTarget = invocationTarget,
                MethodArguments = methodArguments,
                Type = (Type) type
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
            
            return functionInfo.MethodWrapper?.BeforeWrappedMethod(traceMethodInfo);
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
                var assemblyName = traceMethodInfo.Type.Assembly.GetName().Name;
                if (_assemblies.TryGetValue(assemblyName, out var assemblyInfoCache))
                {
                    if (assemblyInfoCache.Assembly == null)
                    {
                        lock (assemblyInfoCache)
                        {
                            if (assemblyInfoCache.Assembly == null)
                            {
                                var assembly = Assembly.Load(assemblyInfoCache.AssemblyName);
                                if (assembly != null)
                                {
                                    assemblyInfoCache.Assembly = assembly;
                                }
                                else
                                {
                                    var home = _traceEnvironment.GetProfilerHome();
                                    var path = Path.Combine(home, $"{assemblyInfoCache.AssemblyName}.dll");
                                    if (File.Exists(path))
                                    {
                                        assembly = Assembly.LoadFile(path);
#if NET
                                        AppDomain.CurrentDomain.Load(assembly.GetName());
#endif
                                        assemblyInfoCache.Assembly = assembly;
                                    }
                                    else
                                    {
                                        throw new FileNotFoundException($"FileNotFound Path:{path}");
                                    }
                                }
                            }
                        }
                    }

                    if (assemblyInfoCache.MethodWrappers == null)
                    {
                        lock (assemblyInfoCache)
                        {
                            if (assemblyInfoCache.MethodWrappers == null)
                            {
                                assemblyInfoCache.MethodWrappers = GetMethodWrappers(assemblyInfoCache.Assembly);
                            }
                        }
                    }

                    foreach (var methodWrapper in assemblyInfoCache.MethodWrappers)
                    {
                        if (methodWrapper.CanWrap(traceMethodInfo))
                        {
                            functionInfo.MethodWrapper = methodWrapper;
                            break;
                        }
                    }
                }
                if (functionInfo.MethodWrapper == null)
                {
                    functionInfo.MethodWrapper = new NoopMethodWrapper(_serviceProvider);
                }
            }
            catch (BadImageFormatException)
            {
                functionInfo.MethodWrapper = new NoopMethodWrapper(_serviceProvider);
            }
            catch (FileNotFoundException)
            {
                functionInfo.MethodWrapper = new NoopMethodWrapper(_serviceProvider);
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

        /// <summary>
        /// GetMethodWrappers
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private List<IMethodWrapper> GetMethodWrappers(Assembly assembly)
        {
            var methodWrappers = new List<IMethodWrapper>();
            var methodWrapperTypes = GetMethodWrapperTypes(assembly);
            foreach (var methodWrapperType in methodWrapperTypes)
            {
                var wrapper = (IMethodWrapper) Activator.CreateInstance(methodWrapperType, _serviceProvider);
                methodWrappers.Add(wrapper);
            }
            return methodWrappers;
        }

        /// <summary>
        /// GetMethodWrapperTypes
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private List<Type> GetMethodWrapperTypes(Assembly assembly)
        {
            List<Type> wrapperTypes = new List<Type>();
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(AbsMethodWrapper).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    {
                        wrapperTypes.Add(type);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
            return wrapperTypes;
        }

        private class FunctionInfoCache
        {
            public IMethodWrapper MethodWrapper { get; set; }

            public MethodBase MethodBase { get; set; }
        }

        private class AssemblyInfoCache
        {
            public Assembly Assembly { get; set; }

            public string AssemblyName { get; set; }

            public List<IMethodWrapper> MethodWrappers { get; set; }
        }
    }
}

