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
using Microsoft.Extensions.Configuration;
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
using SkyApm.Logging;
using SkyApm.PeerFormatters.MySqlConnector;
using SkyApm.PeerFormatters.SqlClient;
using SkyApm.Sampling;
using SkyApm.Service;
using SkyApm.Tracing;
using SkyApm.Transport;
using SkyApm.Utilities.Configuration;
using SkyApm.Utilities.DependencyInjection;
using SkyApm.Utilities.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSkyAPM(this IServiceCollection services,
            Action<SkyApmExtensions> extensionsSetup = null)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null || environment.Length < 1)
            {
                environment = "Development";
            }

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("skyapm.json", true);
            configurationBuilder.AddJsonFile("skyapm." + environment + ".json", true);
            IConfiguration configuration = configurationBuilder.Build();

            services.AddSkyAPM(configuration,extensionsSetup);
            return services;
        }

        public static IServiceCollection AddSkyAPM(this IServiceCollection services, IConfiguration configuration,
          Action<SkyApmExtensions> extensionsSetup = null)
        {
            #region can be optimized

            string enable = configuration?.GetSection("SkyWalking:Enable").Value;
            if (enable != null && "false".Equals(enable.ToLower()))
            {
                return services;
            }

            string serviceName = configuration?.GetSection("SkyWalking:ServiceName").Value ?? "";
            if (null == serviceName || serviceName.Length < 1)
            {
                return services;
            }

            Environment.SetEnvironmentVariable("SKYWALKING__SERVICENAME", serviceName);

            #endregion

            services.AddSkyAPMCore(extensionsSetup);
            return services;
        }

        private static IServiceCollection AddSkyAPMCore(this IServiceCollection services,
            Action<SkyApmExtensions> extensionsSetup = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ISegmentDispatcher, AsyncQueueSegmentDispatcher>();
            services.AddSingleton<IExecutionService, RegisterService>();
            services.AddSingleton<IExecutionService, LogReportService>();
            services.AddSingleton<IExecutionService, PingService>();
            services.AddSingleton<SegmentReportService>();
            services.AddSingleton<IExecutionService, CLRStatsService>();
            services.AddSingleton<IInstrumentStartup, InstrumentStartup>();
            services.AddSingleton<IRuntimeEnvironment>(RuntimeEnvironment.Instance);
            services.AddSingleton<TracingDiagnosticProcessorObserver>();
            services.AddSingleton<IConfigAccessor, ConfigAccessor>();
            services.AddSingleton<IConfigurationFactory, ConfigurationFactory>();
            services.AddSingleton<IHostedService, InstrumentationHostedService>();
            services.AddSingleton<IEnvironmentProvider, HostingEnvironmentProvider>();
            services.AddSingleton<ISkyApmLogDispatcher, AsyncQueueSkyApmLogDispatcher>();
            services.AddSingleton<IPeerFormatter, PeerFormatter>();
            services.AddTracing();
            services.AddSampling();
            services.AddTransport();
            services.AddSkyApmLogging();
            var extensions = services.AddSkyApmExtensions()
                .AddHttpClient()
                .AddGrpcClient()
                .AddSqlClient()
                .AddGrpc()
                .AddEntityFrameworkCore(c => c.AddPomeloMysql().AddNpgsql().AddSqlite())
                .AddMSLogging()
                .AddSqlClientPeerFormatter()
                .AddMySqlConnectorPeerFormatter();

            extensionsSetup?.Invoke(extensions);

            return services;
        }

        private static IServiceCollection AddTracing(this IServiceCollection services)
        {
            services.AddSingleton<ITracingContext, TracingContext>();
            services.AddSingleton<ICarrierPropagator, CarrierPropagator>();
            services.AddSingleton<ICarrierFormatter, Sw8CarrierFormatter>();
            services.AddSingleton<ISegmentContextFactory, SegmentContextFactory>();
            services.AddSingleton<IEntrySegmentContextAccessor, EntrySegmentContextAccessor>();
            services.AddSingleton<ILocalSegmentContextAccessor, LocalSegmentContextAccessor>();
            services.AddSingleton<IExitSegmentContextAccessor, ExitSegmentContextAccessor>();
            services.AddSingleton<ISegmentContextAccessor, SegmentContextAccessor>();
            services.AddSingleton<ISamplerChainBuilder, SamplerChainBuilder>();
            services.AddSingleton<IUniqueIdGenerator, UniqueIdGenerator>();
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
            services.AddSingleton<ISamplingInterceptor, IgnorePathSamplingInterceptor>();
            return services;
        }

        private static IServiceCollection AddTransport(this IServiceCollection services)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null || environment.Length < 1)
            {
                environment = "Development";
            }
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("skyapm.json", true);
            configurationBuilder.AddJsonFile("skyapm." + environment + ".json", true);
            IConfiguration configuration = configurationBuilder.Build();
            string reporter = configuration?.GetSection("SkyWalking:Transport:Reporter").Value ?? "grpc";
            switch (reporter.ToLower())
            {
                case "grpc":
                    services.AddTransportGrpc();
                    break;
                case "kafka":
                    services.AddTransportKafka();
                    break;
                default:
                    services.AddTransportGrpc();
                    break;
            }
            return services;
        }

        private static IServiceCollection AddTransportGrpc(this IServiceCollection services)
        {
            services.AddSingleton<ISegmentReporter, SkyApm.Transport.Grpc.SegmentReporter>();
            services.AddSingleton<ILogReporter, SkyApm.Transport.Grpc.LogReporter>();
            services.AddSingleton<ICLRStatsReporter, SkyApm.Transport.Grpc.CLRStatsReporter>();
            services.AddSingleton<SkyApm.Transport.Grpc.ConnectionManager>();
            services.AddSingleton<IPingCaller, SkyApm.Transport.Grpc.PingCaller>();
            services.AddSingleton<IServiceRegister, SkyApm.Transport.Grpc.ServiceRegister>();
            services.AddSingleton<IExecutionService, SkyApm.Transport.Grpc.ConnectService>();
            return services;
        }

        private static IServiceCollection AddTransportKafka(this IServiceCollection services)
        {
            services.AddSingleton<ISegmentReporter, SkyApm.Transport.Kafka.SegmentReporter>();
            services.AddSingleton<ILogReporter, SkyApm.Transport.Kafka.LogReporter>();
            services.AddSingleton<ICLRStatsReporter, SkyApm.Transport.Kafka.CLRStatsReporter>();
            services.AddSingleton<IPingCaller, SkyApm.Transport.Kafka.PingCaller>();
            services.AddSingleton<IServiceRegister, SkyApm.Transport.Kafka.ServiceRegister>();
            return services;
        }

        private static IServiceCollection AddSkyApmLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
            return services;
        }
    }
}
