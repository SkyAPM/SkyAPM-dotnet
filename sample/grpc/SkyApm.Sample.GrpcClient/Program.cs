﻿/*
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
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Agent.GeneralHost;

namespace SkyApm.Sample.GrpcClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Services.StartGrpcClient();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new HostBuilder()
                .UseEnvironment(environmentName)
                .AddSkyAPM()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                });
        }
    }
}
