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
using Microsoft.Extensions.DependencyInjection;
using SkyApm.ClrProfiler.Trace.Logging;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Sampling;
using SkyApm.Service;
using SkyApm.Tracing;
using SkyApm.Transport;

namespace SkyApm.ClrProfiler.Trace.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSkyAPMCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ISegmentDispatcher, AsyncQueueSegmentDispatcher>();
            services.AddSingleton<IExecutionService, RegisterService>();
            services.AddSingleton<IExecutionService, PingService>();
            services.AddSingleton<IExecutionService, ServiceDiscoveryV5Service>();
            services.AddSingleton<IExecutionService, SegmentReportService>();
            services.AddSingleton<IInstrumentStartup, InstrumentStartup>();
            services.AddSingleton<IRuntimeEnvironment>(RuntimeEnvironment.Instance);
            services.AddSingleton<IConfigAccessor, ConfigAccessor>();
            services.AddSingleton<IConfigurationFactory, ConfigurationFactory>();
            services.AddTracing().AddSampling().AddLogging().AddTransport();
            return services;
        }

        private static IServiceCollection AddTracing(this IServiceCollection services)
        {
            services.AddSingleton<ITracingContext, TracingContext>();
            services.AddSingleton<ICarrierPropagator, CarrierPropagator>();
            services.AddSingleton<ICarrierFormatter, Sw3CarrierFormatter>();
            services.AddSingleton<ICarrierFormatter, Sw6CarrierFormatter>();
            services.AddSingleton<ISegmentContextFactory, SegmentContextFactory>();
            services.AddSingleton<ISegmentContextScopeManager, SegmentContextScopeManager>();
            services.AddSingleton<ISamplerChainBuilder, SamplerChainBuilder>();
            services.AddSingleton<IUniqueIdGenerator, UniqueIdGenerator>();
            services.AddSingleton<IUniqueIdParser, UniqueIdParser>();
            services.AddSingleton<ISegmentContextMapper, SegmentContextMapper>();
            services.AddSingleton<IBase64Formatter, Base64Formatter>();
            return services;
        }

        private static IServiceCollection AddSampling(this IServiceCollection services)
        {
            services.AddSingleton<SimpleCountSamplingInterceptor>();
            services.AddSingleton<ISamplingInterceptor>(p => p.GetService<SimpleCountSamplingInterceptor>());
            services.AddSingleton<IExecutionService>(p => p.GetService<SimpleCountSamplingInterceptor>());
            services.AddSingleton<ISamplingInterceptor, RandomSamplingInterceptor>();
            return services;
        }

        private static IServiceCollection AddLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
            return services;
        }

        /// <summary>
        /// Grpc native dll can't custom load path
        /// when load from GAC will raise error 
        /// so use reflect load SkyApm.Transport.Grpc.dll(packed) from profilerHome
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection AddTransport(this IServiceCollection services)
        {
            var profilerHome = TraceEnvironment.Instance.GetProfilerHome();
            if (string.IsNullOrEmpty(profilerHome))
            {
                throw new ArgumentNullException(nameof(profilerHome));
            }

            var filepath = Path.Combine(profilerHome, "SkyAPM.Transport.Grpc.dll");
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
            return services;
        }

        /// <summary>
        /// AddMethodWrapperTypes
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddMethodWrapperTypes(this IServiceCollection services)
        {
            var profilerHome = TraceEnvironment.Instance.GetProfilerHome();
            foreach (var dllPath in Directory.GetFiles(profilerHome, "*.dll"))
            {
                try
                {
                    var fileName = Path.GetFileName(dllPath);
                    if (fileName.StartsWith("SkyApm.ClrProfiler.Trace", StringComparison.OrdinalIgnoreCase))
                    {
                        var assembly = Assembly.Load(fileName.Replace(".dll", ""));
                        if (assembly != null)
                        {
                            AddMethodWrapperTypes(assembly);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                }
            }

            void AddMethodWrapperTypes(Assembly assembly)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(AbsMethodWrapper).IsAssignableFrom(type) &&
                        type.IsClass && !type.IsAbstract && 
                        type != typeof(NoopMethodWrapper))
                    {
                        services.AddSingleton(typeof(IMethodWrapper), type);
                    }
                }
            }

            return services;
        }
    }
}

