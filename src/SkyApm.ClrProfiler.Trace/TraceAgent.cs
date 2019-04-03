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
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SkyApm.ClrProfiler.Trace.DependencyInjection;
using SkyApm.ClrProfiler.Trace.Extensions;

namespace SkyApm.ClrProfiler.Trace
{
    public delegate void AfterMethodDelegate(object returnValue, Exception ex);

    public class TraceAgent
    {
        private static readonly TraceAgent Instance = new TraceAgent();

        private readonly bool _initialized = false;

        private TraceAgent()
        {
            try
            {
                var profilerHome = TraceEnvironment.Instance.GetProfilerHome();
                if (string.IsNullOrEmpty(profilerHome))
                {
                    throw new ArgumentException("CLR PROFILER HOME IsNullOrEmpty");
                }

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                ServiceLocator.Instance.RegisterServices(RegisterServices);

                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

                ServiceLocator.Instance.GetService<MethodTraceFinderService>();

                Task.Run(() => { ServiceLocator.Instance.GetService<IInstrumentStartup>()?.StartAsync(); });

                _initialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                _initialized = false;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var profilerHome = TraceEnvironment.Instance.GetProfilerHome();
            if (!string.IsNullOrEmpty(profilerHome))
            {
                var filepath = Path.Combine(profilerHome, $"{new AssemblyName(args.Name).Name}.dll");
                if (File.Exists(filepath))
                {
                    return Assembly.LoadFrom(filepath);
                }
            }
            return null;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            try
            {
                var instrumentStartup = ServiceLocator.Instance.GetService<IInstrumentStartup>();
                instrumentStartup?.StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // ignored
            }
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddMethodWrapperTypes()
                .AddSingleton(TraceEnvironment.Instance)
                .AddSingleton<MethodTraceFinderService>()
                .AddSkyAPMCore();
        }

        public static object GetInstance()
        {
            return Instance;
        }

        public object BeforeMethod(object type, object invocationTarget, object[] methodArguments, uint functionToken)
        {
            if (!_initialized)
            {
                return default(MethodTrace);
            }

            var args = methodArguments;
            var methodTraceFinderService = ServiceLocator.Instance.GetService<MethodTraceFinderService>();
            return methodTraceFinderService.GetMethodTrace(type, invocationTarget, args, functionToken);
        }
    }

    public class MethodTrace
    {
        private readonly AfterMethodDelegate _afterMethodDelegate;

        public MethodTrace(AfterMethodDelegate afterMethodDelegate)
        {
            this._afterMethodDelegate = afterMethodDelegate;
        }

        public void AfterMethod(object returnValue, object ex)
        {
            this._afterMethodDelegate(returnValue, (Exception)ex);
        }
    }
}

