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
using System.Linq;

namespace SkyApm.Config
{
    [Config("SkyWalking", "Transport", "gRPC")]
    public class GrpcConfig
    {
        public string Servers { get; set; }

        public int ConnectTimeout { get; set; }

        public int Timeout { get; set; }

        public int ReportTimeout { get; set; }

        public string Authentication { get; set; }
    }

    public static class GrpcConfigExtensions
    {
        public static string[] GetServers(this GrpcConfig config)
        {
            var servers = config.Servers.Split(',').ToArray();
            for (int i = 0; i < servers.Length; i++)
            {
                if (!servers[i].StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        && !servers[i].StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    servers[i] = $"http://{servers[i]}";
                }
            } 

            return servers;
        }

        public static DateTime GetTimeout(this GrpcConfig config)
        {
            return DateTime.UtcNow.AddMilliseconds(config.Timeout);
        }
        
        public static DateTime GetConnectTimeout(this GrpcConfig config)
        {
            return DateTime.UtcNow.AddMilliseconds(config.ConnectTimeout);
        }
        
        public static DateTime GetReportTimeout(this GrpcConfig config)
        {
            return DateTime.UtcNow.AddMilliseconds(config.ReportTimeout);
        }
    }
}