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
using SkyApm.ClrProfiler.Trace.Logging;
using SkyApm.Logging;
using SkyApm.Transport;
using SkyWalking.Extensions;

namespace SkyApm.ClrProfiler.Trace
{
    public delegate void EndMethodDelegate(object returnValue, Exception ex);

    public class TraceAgent
    {
        private static readonly TraceAgent Instance = new TraceAgent();

        private readonly bool _initialized = false;

        private TraceAgent()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                ServiceLocator.Instance.RegisterServices(RegisterServices);

                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

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
            var instrumentStartup = ServiceLocator.Instance.GetService<IInstrumentStartup>();
            instrumentStartup?.StopAsync().GetAwaiter().GetResult();
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(TraceEnvironment.Instance);
            services.AddSingleton<MethodFinderService>();

            services.AddSkyAPMCore();

            AddSkyAPMTransport(services);

            services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
        }
        
        /// <summary>
        /// Grpc native dll can't custom load path
        /// when load from GAC will raise error 
        /// so use reflect load SkyApm.Transport.Grpc.dll(packed) from profilerHome
        /// </summary>
        /// <param name="services"></param>
        private void AddSkyAPMTransport(IServiceCollection services)
        {
            var profilerHome = TraceEnvironment.Instance.GetProfilerHome();
            if (string.IsNullOrEmpty(profilerHome))
            {
                throw new ArgumentNullException(nameof(profilerHome));
            }

            var filepath = Path.Combine(profilerHome, "SkyApm.Transport.Grpc.dll");
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"{filepath} not found");
            }

            var assembly = Assembly.LoadFrom(filepath);
            services.AddSingleton(typeof(ISkyApmClientV5), assembly.GetType("SkyApm.Transport.Grpc.V5.SkyApmClientV5"));
            services.AddSingleton(typeof(ISegmentReporter), assembly.GetType("SkyApm.Transport.Grpc.SegmentReporter"));
            services.AddSingleton(typeof(IPingCaller), assembly.GetType("SkyApm.Transport.Grpc.V6.PingCaller"));
            services.AddSingleton(typeof(IServiceRegister), assembly.GetType("SkyApm.Transport.Grpc.V6.ServiceRegister"));
            services.AddSingleton(typeof(IExecutionService), assembly.GetType("SkyApm.Transport.Grpc.V6.ConnectService"));
            services.AddSingleton(assembly.GetType("SkyApm.Transport.Grpc.ConnectionManager"));
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

            try
            {
                var args = methodArguments;
                var wrapperService = ServiceLocator.Instance.GetService<MethodFinderService>();
                var endMethodDelegate = wrapperService.BeforeWrappedMethod(type, invocationTarget, args, functionToken);
                return endMethodDelegate != null ? new MethodTrace(endMethodDelegate) : default(MethodTrace);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return default(MethodTrace);
            }
        }
    }

    public class MethodTrace
    {
        private readonly EndMethodDelegate _endMethodDelegate;

        public MethodTrace(EndMethodDelegate endMethodDelegate)
        {
            this._endMethodDelegate = endMethodDelegate;
        }

        public void EndMethod(object returnValue, object ex)
        {
            this._endMethodDelegate(returnValue, (Exception)ex);
        }
    }
}

