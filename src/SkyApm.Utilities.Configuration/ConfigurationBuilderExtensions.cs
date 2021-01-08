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
                {"SkyWalking:Namespace", configuration?.GetSection("SkyWalking:Namespace").Value ?? string.Empty },
                {"SkyWalking:ServiceName", configuration?.GetSection("SkyWalking:ServiceName").Value ?? "My_Service" },
                {"Skywalking:ServiceInstanceName", configuration?.GetSection("SkyWalking:ServiceInstanceName").Value ?? BuildDefaultServiceInstanceName() },
                {"SkyWalking:HeaderVersions:0", configuration?.GetSection("SkyWalking:HeaderVersions:0").Value ?? HeaderVersions.SW8 },
                {"SkyWalking:Sampling:SamplePer3Secs", configuration?.GetSection("SkyWalking:Sampling:SamplePer3Secs").Value ?? "-1" },
                {"SkyWalking:Sampling:Percentage", configuration?.GetSection("SkyWalking:Sampling:Percentage").Value ?? "-1" },
                {"SkyWalking:Logging:Level",  configuration?.GetSection("SkyWalking:Logging:Level").Value ?? "Information" },
                {"SkyWalking:Logging:FilePath", configuration?.GetSection("SkyWalking:Logging:FilePath").Value ?? defaultLogFile },
                {"SkyWalking:Transport:Interval", configuration?.GetSection("SkyWalking:Transport:Interval").Value ?? "3000" },
                {"SkyWalking:Transport:ProtocolVersion", configuration?.GetSection("SkyWalking:Transport:ProtocolVersion").Value ?? ProtocolVersions.V8 },
                {"SkyWalking:Transport:QueueSize", configuration?.GetSection("SkyWalking:Transport:QueueSize").Value ?? "30000" },
                {"SkyWalking:Transport:BatchSize", configuration?.GetSection("SkyWalking:Transport:BatchSize").Value ?? "3000" },
                {"SkyWalking:Transport:gRPC:Servers",configuration?.GetSection("SkyWalking:Transport:gRPC:Servers").Value ?? "localhost:11800" },
                {"SkyWalking:Transport:gRPC:Timeout",configuration?.GetSection("SkyWalking:Transport:gRPC:Timeout").Value ?? "10000" },
                {"SkyWalking:Transport:gRPC:ReportTimeout",configuration?.GetSection("SkyWalking:Transport:gRPC:ReportTimeout").Value ?? "600000" },
                {"SkyWalking:Transport:gRPC:ConnectTimeout",configuration?.GetSection("SkyWalking:Transport:gRPC:ConnectTimeout").Value ?? "10000" }
            };
            return builder.AddInMemoryCollection(defaultConfig);
        }

        /// <summary>
        /// Try append an ip to the instancename to make it more meaningful
        /// </summary>
        /// <returns></returns>
        private static string BuildDefaultServiceInstanceName()
        {
            var guid = Guid.NewGuid().ToString("N");
            try
            {
                var hostName = Dns.GetHostName();
                var ipAddress = Dns.GetHostAddresses(hostName)
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                    .First().ToString();

                return $"{ipAddress}@{guid}";
            }
            catch (Exception)
            {
                return guid;
            }
        }
    }
}
