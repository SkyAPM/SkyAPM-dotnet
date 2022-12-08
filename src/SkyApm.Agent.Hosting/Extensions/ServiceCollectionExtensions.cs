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

using Microsoft.Extensions.Hosting;
using SkyApm;
using SkyApm.Agent.Hosting;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Diagnostics.EntityFrameworkCore;
using SkyApm.Diagnostics.Grpc;
using SkyApm.Diagnostics.Grpc.Net.Client;
using SkyApm.Diagnostics.HttpClient;
using SkyApm.Diagnostics.MSLogging;
using SkyApm.Diagnostics.SqlClient;
using SkyApm.Sampling;
using SkyApm.Service;
using SkyApm.Tracing;
using SkyApm.Transport;
using SkyApm.Transport.Grpc;
using SkyApm.Utilities.Configuration;
using SkyApm.Utilities.DependencyInjection;
using SkyApm.Utilities.Logging;
using System;
using ILoggerFactory = SkyApm.Logging.ILoggerFactory;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSkyAPM(this IServiceCollection services, Action<SkyApmExtensions> extensionsSetup = null)
    {
        _ = services.AddSkyAPMCore(extensionsSetup);
        return services;
    }

    private static IServiceCollection AddSkyAPMCore(this IServiceCollection services, Action<SkyApmExtensions> extensionsSetup = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        _ = services.AddSingleton<ISegmentDispatcher, AsyncQueueSegmentDispatcher>();
        _ = services.AddSingleton<IExecutionService, RegisterService>();
        _ = services.AddSingleton<IExecutionService, LogReportService>();
        _ = services.AddSingleton<IExecutionService, PingService>();
        _ = services.AddSingleton<IExecutionService, SegmentReportService>();
        _ = services.AddSingleton<IExecutionService, CLRStatsService>();
        _ = services.AddSingleton<IInstrumentStartup, InstrumentStartup>();
        _ = services.AddSingleton(RuntimeEnvironment.Instance);
        _ = services.AddSingleton<TracingDiagnosticProcessorObserver>();
        _ = services.AddSingleton<IConfigAccessor, ConfigAccessor>();
        _ = services.AddSingleton<IConfigurationFactory, ConfigurationFactory>();
        _ = services.AddSingleton<IHostedService, InstrumentationHostedService>();
        _ = services.AddSingleton<IEnvironmentProvider, HostingEnvironmentProvider>();
        _ = services.AddSingleton<ISkyApmLogDispatcher, AsyncQueueSkyApmLogDispatcher>();
        _ = services.AddTracing().AddSampling().AddGrpcTransport().AddSkyApmLogging();
        var extensions = services.AddSkyApmExtensions()
            .AddHttpClient()
            .AddGrpcClient()
            .AddSqlClient()
            .AddGrpc()
            .AddEntityFrameworkCore(c => c.AddPomeloMysql().AddNpgsql().AddSqlite())
            .AddMSLogging();

        extensionsSetup?.Invoke(extensions);

        return services;
    }

    private static IServiceCollection AddTracing(this IServiceCollection services)
    {
        _ = services.AddSingleton<ITracingContext, TracingContext>();
        _ = services.AddSingleton<ICarrierPropagator, CarrierPropagator>();
        _ = services.AddSingleton<ICarrierFormatter, Sw8CarrierFormatter>();
        _ = services.AddSingleton<ISegmentContextFactory, SegmentContextFactory>();
        _ = services.AddSingleton<IEntrySegmentContextAccessor, EntrySegmentContextAccessor>();
        _ = services.AddSingleton<ILocalSegmentContextAccessor, LocalSegmentContextAccessor>();
        _ = services.AddSingleton<IExitSegmentContextAccessor, ExitSegmentContextAccessor>();
        _ = services.AddSingleton<ISegmentContextAccessor, SegmentContextAccessor>();
        _ = services.AddSingleton<ISamplerChainBuilder, SamplerChainBuilder>();
        _ = services.AddSingleton<IUniqueIdGenerator, UniqueIdGenerator>();
        _ = services.AddSingleton<ISegmentContextMapper, SegmentContextMapper>();
        _ = services.AddSingleton<IBase64Formatter, Base64Formatter>();
        return services;
    }

    private static IServiceCollection AddSampling(this IServiceCollection services)
    {
        _ = services.AddSingleton<SimpleCountSamplingInterceptor>();
        _ = services.AddSingleton<ISamplingInterceptor>(p => p.GetService<SimpleCountSamplingInterceptor>());
        _ = services.AddSingleton<IExecutionService>(p => p.GetService<SimpleCountSamplingInterceptor>());
        _ = services.AddSingleton<ISamplingInterceptor, RandomSamplingInterceptor>();
        _ = services.AddSingleton<ISamplingInterceptor, IgnorePathSamplingInterceptor>();
        return services;
    }

    private static IServiceCollection AddGrpcTransport(this IServiceCollection services)
    {
        _ = services.AddSingleton<ISegmentReporter, SegmentReporter>();
        _ = services.AddSingleton<ILoggerReporter, LoggerReporter>();
        _ = services.AddSingleton<ICLRStatsReporter, CLRStatsReporter>();
        _ = services.AddSingleton<ConnectionManager>();
        _ = services.AddSingleton<IPingCaller, PingCaller>();
        _ = services.AddSingleton<IServiceRegister, ServiceRegister>();
        _ = services.AddSingleton<IExecutionService, ConnectService>();
        return services;
    }

    private static IServiceCollection AddSkyApmLogging(this IServiceCollection services)
    {
        _ = services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
        return services;
    }

}