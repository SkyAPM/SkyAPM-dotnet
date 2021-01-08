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

using Microsoft.Extensions.DependencyInjection;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Logging;
using SkyApm.Sampling;
using SkyApm.Service;
using SkyApm.Tracing;
using SkyApm.Transport;
using SkyApm.Transport.Grpc;
using SkyApm.Utilities.Configuration;
using SkyApm.Utilities.Logging;

namespace SkyApm.Agent.AspNet.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSkyAPMCore(this IServiceCollection services)
        {
            services.AddSingleton<ISegmentDispatcher, AsyncQueueSegmentDispatcher>();
            services.AddSingleton<IExecutionService, RegisterService>();
            services.AddSingleton<IExecutionService, PingService>();
            services.AddSingleton<IExecutionService, SegmentReportService>();
            services.AddSingleton<IExecutionService, CLRStatsService>();
            services.AddSingleton<IInstrumentStartup, InstrumentStartup>();
            services.AddSingleton<IRuntimeEnvironment>(RuntimeEnvironment.Instance);
            services.AddSingleton<TracingDiagnosticProcessorObserver>();
            services.AddSingleton<IConfigAccessor, ConfigAccessor>();
            services.AddSingleton<IEnvironmentProvider, HostingEnvironmentProvider>();
            services.AddSingleton<InstrumentRequestCallback>();
            services.AddSingleton<IConfigurationFactory, SkyApm.Agent.AspNet.Configuration.ConfigurationFactory>();

            services.AddSingleton<ITracingContext, Tracing.TracingContext>();
            services.AddSingleton<ICarrierPropagator, CarrierPropagator>();
            services.AddSingleton<ICarrierFormatter, Sw8CarrierFormatter>();
            services.AddSingleton<ISegmentContextFactory, SegmentContextFactory>();
            services.AddSingleton<IEntrySegmentContextAccessor, SkyApm.AspNet.Tracing.EntrySegmentContextAccessor>();
            services.AddSingleton<ILocalSegmentContextAccessor, SkyApm.AspNet.Tracing.LocalSegmentContextAccessor>();
            services.AddSingleton<IExitSegmentContextAccessor, SkyApm.AspNet.Tracing.ExitSegmentContextAccessor>();
            services.AddSingleton<ISamplerChainBuilder, SamplerChainBuilder>();
            services.AddSingleton<IUniqueIdGenerator, UniqueIdGenerator>();
            services.AddSingleton<ISegmentContextMapper, SegmentContextMapper>();
            services.AddSingleton<IBase64Formatter, Base64Formatter>();

            services.AddSingleton<SimpleCountSamplingInterceptor>();
            services.AddSingleton<ISamplingInterceptor>(p => p.GetService<SimpleCountSamplingInterceptor>());
            services.AddSingleton<IExecutionService>(p => p.GetService<SimpleCountSamplingInterceptor>());
            services.AddSingleton<ISamplingInterceptor, RandomSamplingInterceptor>();
            services.AddSingleton<ISamplingInterceptor, IgnorePathSamplingInterceptor>();

            services.AddSingleton<ISegmentReporter, SegmentReporter>();
            services.AddSingleton<ICLRStatsReporter, CLRStatsReporter>();
            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<IPingCaller, PingCaller>();
            services.AddSingleton<IServiceRegister, ServiceRegister>();
            services.AddSingleton<IExecutionService, ConnectService>();

            services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
            return services;
        }
    }
}