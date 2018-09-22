/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyWalking.AspNetCore.Diagnostics;
using SkyWalking.Config;
using SkyWalking.Diagnostics;
using SkyWalking.Diagnostics.EntityFrameworkCore;
using SkyWalking.Diagnostics.HttpClient;
using SkyWalking.Diagnostics.SqlClient;
using SkyWalking.Extensions.Configuration;
using SkyWalking.Extensions.DependencyInjection;
using SkyWalking.Extensions.Logging;
using SkyWalking.Logging;
using SkyWalking.Service;
using SkyWalking.Transport;
using SkyWalking.Transport.Grpc;

[assembly:InternalsVisibleTo("SkyWalking.Sample.Frontend")]
[assembly:InternalsVisibleTo("SkyWalking.Sample.Backend")]

namespace SkyWalking.AspNetCore
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSkyWalkingCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ITraceDispatcher, BlockingTraceDispatcher>();
            services.AddSingleton<IInstrumentationService, TraceSegmentTransportService>();
            services.AddSingleton<IInstrumentationService, ServiceDiscoveryService>();
            services.AddSingleton<IInstrumentationService, SamplingRefreshService>();
            services.AddSingleton<IInstrumentationServiceStartup, InstrumentationServiceStartup>();
            services.AddSingleton<ISampler>(DefaultSampler.Instance);
            services.AddSingleton(RuntimeEnvironment.Instance);
            services.AddSingleton<TracingDiagnosticProcessorObserver>();
            services.AddSingleton<IConfigAccessor, ConfigAccessor>();
            services.AddSingleton<IHostedService, InstrumentationHostedService>();
            services.AddSingleton<IEnvironmentProvider, HostingEnvironmentProvider>();
            services.AddGrpcTransport().AddLogging();
            services.AddSkyWalkingExtensions().AddAspNetCoreHosting().AddHttpClient().AddSqlClient().AddEntityFrameworkCore(c => c.AddSqlite().AddPomeloMysql().AddNpgsql());
            return services;
        }

        private static IServiceCollection AddGrpcTransport(this IServiceCollection services)
        {
            services.AddSingleton<IInstrumentationClient, GrpcInstrumentationClient>();
            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<IInstrumentationService, GrpcStateCheckService>();
            return services;
        }
        
        private static IServiceCollection AddLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
            return services;
        }
    }
}