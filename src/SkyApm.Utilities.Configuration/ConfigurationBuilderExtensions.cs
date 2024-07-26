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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using SkyApm.Config;

namespace SkyApm.Utilities.Configuration
{
    internal static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddSkyWalkingDefaultConfig(this IConfigurationBuilder builder, IConfiguration configuration)
        {
            var defaultLogFile = Path.Combine("logs", "skyapm-{Date}.log");
            var defaultConfig = new Dictionary<string, string>
            {
                { "SkyWalking:Enable", configuration?.GetSection("SkyWalking:Enable").Value ?? "true" },
                { "SkyWalking:Namespace", configuration?.GetSection("SkyWalking:Namespace").Value ?? string.Empty },
                { "SkyWalking:ServiceName", configuration?.GetSection("SkyWalking:ServiceName").Value ?? "My_Service" },
                { "Skywalking:ServiceInstanceName", configuration?.GetSection("SkyWalking:ServiceInstanceName").Value ?? BuildDefaultServiceInstanceName() },
                { "SkyWalking:HeaderVersions:0", configuration?.GetSection("SkyWalking:HeaderVersions:0").Value ?? HeaderVersions.SW8 },
                { "SkyWalking:Sampling:SamplePer3Secs", configuration?.GetSection("SkyWalking:Sampling:SamplePer3Secs").Value ?? "-1" },
                { "SkyWalking:Sampling:Percentage", configuration?.GetSection("SkyWalking:Sampling:Percentage").Value ?? "-1" },
                { "SkyWalking:Logging:Level",  configuration?.GetSection("SkyWalking:Logging:Level").Value ?? "Information" },
                { "SkyWalking:Logging:FilePath", configuration?.GetSection("SkyWalking:Logging:FilePath").Value ?? defaultLogFile },
                { "SkyWalking:Logging:FileSizeLimitBytes", configuration?.GetSection("SkyWalking:Logging:FileSizeLimitBytes").Value ?? "268435456" },
                { "SkyWalking:Logging:FlushToDiskInterval", configuration?.GetSection("SkyWalking:Logging:FlushToDiskInterval").Value ?? "1000" },
                { "SkyWalking:Logging:RollingInterval", configuration?.GetSection("SkyWalking:Logging:RollingInterval").Value ?? "Day" },
                { "SkyWalking:Logging:RollOnFileSizeLimit", configuration?.GetSection("SkyWalking:Logging:RollOnFileSizeLimit").Value ?? "false" },
                { "SkyWalking:Logging:RetainedFileCountLimit", configuration?.GetSection("SkyWalking:Logging:RetainedFileCountLimit").Value ?? "10" },
                { "SkyWalking:Logging:RetainedFileTimeLimit", configuration?.GetSection("SkyWalking:Logging:RetainedFileTimeLimit").Value ?? "864000000" },
                { "Skywalking:MeterActive", configuration?.GetSection("SkyWalking:MeterActive").Value ?? "true" },
                { "SkyWalking:MetricActive", configuration?.GetSection("SkyWalking:MetricActive").Value ?? "true" },
                { "SkyWalking:SegmentActive", configuration?.GetSection("SkyWalking:SegmentActive").Value ?? "true" },
                { "SkyWalking:ProfilingActive", configuration?.GetSection("SkyWalking:ProfilingActive").Value ?? "true" },
                { "SkyWalking:ManagementActive",  configuration?.GetSection("SkyWalking:ManagementActive").Value ?? "true" },
                { "SkyWalking:LogActive", configuration?.GetSection("SkyWalking:LogActive").Value ?? "true" },
                { "SkyWalking:Transport:ProtocolVersion", configuration?.GetSection("SkyWalking:Transport:ProtocolVersion").Value ?? ProtocolVersions.V8 },
                { "SkyWalking:Transport:QueueSize", configuration?.GetSection("SkyWalking:Transport:QueueSize").Value ?? "10000" },
                { "SkyWalking:Transport:BatchSize", configuration?.GetSection("SkyWalking:Transport:BatchSize").Value ?? "2000" },
                { "SkyWalking:Transport:Parallel", configuration?.GetSection("SkyWalking:Transport:Parallel").Value ?? "5" },
                { "SkyWalking:Transport:Interval", configuration?.GetSection("SkyWalking:Transport:Interval").Value ?? "50" },
                { "SkyWalking:Transport:Reporter", configuration?.GetSection("SkyWalking:Transport:Reporter").Value ?? "grpc" },
                { "SkyWalking:Transport:gRPC:Servers", configuration?.GetSection("SkyWalking:Transport:gRPC:Servers").Value ?? "localhost:11800" },
                { "SkyWalking:Transport:gRPC:Timeout", configuration?.GetSection("SkyWalking:Transport:gRPC:Timeout").Value ?? "10000" },
                { "SkyWalking:Transport:gRPC:ReportTimeout", configuration?.GetSection("SkyWalking:Transport:gRPC:ReportTimeout").Value ?? "600000" },
                { "SkyWalking:Transport:gRPC:ConnectTimeout", configuration?.GetSection("SkyWalking:Transport:gRPC:ConnectTimeout").Value ?? "10000" },
                { "SkyWalking:Transport:Kafka:BootstrapServers", configuration?.GetSection("SkyWalking:Transport:Kafka:BootstrapServers").Value ?? "localhost:9092" },
                { "SkyWalking:Transport:Kafka:TopicTimeoutMs", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicTimeoutMs").Value ?? "3000" },
                { "SkyWalking:Transport:Kafka:MessageTimeoutMs", configuration?.GetSection("SkyWalking:Transport:Kafka:MessageTimeoutMs").Value ?? "5000" },
                { "SkyWalking:Transport:Kafka:TopicMeters", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicMeters").Value ?? "skywalking-meters" },
                { "SkyWalking:Transport:Kafka:TopicCLRMetrics", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicCLRMetrics").Value ?? "skywalking-clr-metrics" },
                { "SkyWalking:Transport:Kafka:TopicSegments", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicSegments").Value ?? "skywalking-segments" },
                { "SkyWalking:Transport:Kafka:TopicProfilings", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicProfilings").Value ?? "skywalking-profilings" },
                { "SkyWalking:Transport:Kafka:TopicManagements", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicManagements").Value ?? "skywalking-managements" },
                { "SkyWalking:Transport:Kafka:TopicLogs", configuration?.GetSection("SkyWalking:Transport:Kafka:TopicLogs").Value ?? "skywalking-logs" },
                { "SkyWalking:Component:HttpClient:IgnorePaths:0", configuration?.GetSection("SkyWalking:Component:HttpClient:IgnorePaths:0").Value ?? "" }
            };
            return builder.AddInMemoryCollection(defaultConfig);
        }

        /// <summary>
        /// Try append an ip to the instanceName to make it more meaningful
        /// </summary>
        /// <returns></returns>
        private static string BuildDefaultServiceInstanceName()
        {
            var guid = Guid.NewGuid().ToString("N");
            try
            {
                var hostName = Dns.GetHostName();
                var ipAddress = Dns
                    .GetHostAddresses(hostName)
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();

                return $"{guid}@{ipAddress}";
            }
            catch (Exception)
            {
                return guid;
            }
        }
    }
}
