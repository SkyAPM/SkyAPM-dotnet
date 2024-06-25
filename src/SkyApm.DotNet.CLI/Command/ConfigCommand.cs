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
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using SkyApm.DotNet.CLI.Extensions;

// ReSharper disable ConvertToLocalFunction

// ReSharper disable StringLiteralTypo
namespace SkyApm.DotNet.CLI.Command
{
    public class ConfigCommand : IAppCommand
    {
        private const string GRPC = "grpc";

        private const string KAFKA = "kafka";

        public string Name { get; } = "config";

        public void Execute(CommandLineApplication command)
        {
            command.Description = "Generate config file for SkyApm-dotnet Agent.";
            command.HelpOption();

            var serviceNameArgument = command.Argument(
                "service",
                "[Required] The ServiceName in SkyAPM");
            var reporterOption = command.Option(
                "--reporter",
                "[Optional] The reporter type, default 'grpc'",
                CommandOptionType.SingleValue);
            var grpcServersOption = command.Option(
                "--grpcservers",
                "[Optional] The grpc servers address, default 'localhost:11800'",
                CommandOptionType.SingleValue);
            var kafkaServersOption = command.Option(
                "--kafkaservers",
                "[Optional] The kafka servers address, default 'localhost:9092'",
                CommandOptionType.SingleValue);
            var environmentOption = command.Option(
                "-e|--Environment",
                "Follow the app's environment.Framework-defined values include Development, Staging, and Production",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(serviceNameArgument.Value))
                {
                    Console.WriteLine("Invalid ServiceName.");
                    return 1;
                }

                Generate
                (
                    serviceNameArgument.Value,
                    reporterOption.Value(),
                    grpcServersOption.Value(),
                    kafkaServersOption.Value(),
                    environmentOption.Value());

                return 0;
            });
        }

        private void Generate
        (
            string serviceName,
            string reporter,
            string grpcServers,
            string kafkaServers,
            string environment)
        {
            Func<string, string> configFileName =
                env => string.IsNullOrEmpty(env) ? "skyapm.json" : $"skyapm.{env}.json";

            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), configFileName(environment));

            var configFile = new FileInfo(configFilePath);

            if (configFile.Exists)
            {
                Console.WriteLine("Already exist config file in {0}", configFilePath);
                return;
            }

            if (! GRPC.Equals(reporter, StringComparison.OrdinalIgnoreCase) &&
                ! KAFKA.Equals(reporter, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Invalid reporter type {0}. Use default type.", reporter);
                reporter = GRPC;
            }

            var transportConfig = new Dictionary<string, dynamic>
            {
                { "ProtocolVersion", "v8" },
                { "QueueSize", 10000 },
                { "BatchSize", 2000 },
                { "Parallel", 5 },
                { "Interval", 50 },
                { "Reporter", reporter.ToLower() },
            };

            {
                grpcServers = grpcServers ?? "localhost:11800";
                var grpcConfig = new Dictionary<string, dynamic>
                {
                    { "Servers", grpcServers },
                    { "Timeout", 10000 },
                    { "ConnectTimeout", 10000 },
                    { "ReportTimeout", 600000 },
                    { "Authentication", "" }
                };
                transportConfig.Add("gRPC", grpcConfig);
            }

            {
                kafkaServers = kafkaServers ?? "localhost:9092";
                var kafkaConfig = new Dictionary<string, dynamic>
                {
                    { "BootstrapServers", kafkaServers },
                    { "TopicTimeoutMs", 3000 },
                    { "MessageTimeoutMs", 5000 },
                    { "TopicMeters", "skywalking-meters" },
                    { "TopicCLRMetrics", "skywalking-clr-metrics" },
                    { "TopicSegments", "skywalking-segments" },
                    { "TopicProfilings", "skywalking-profilings" },
                    { "TopicManagements", "skywalking-managements" },
                    { "TopicLogs", "skywalking-logs" }
                };
                transportConfig.Add("Kafka", kafkaConfig);
            }

            var loggingConfig = new Dictionary<string, dynamic>
            {
                { "Level", "Information" },
                { "FilePath", Path.Combine("logs", "skyapm-{Date}.log") }
            };

            var samplingConfig = new Dictionary<string, dynamic>
            {
                { "SamplePer3Secs", -1 },
                { "Percentage", -1d }
            };

            var HeaderVersionsConfig = new string[]
            {
                "sw8"
            };

            var skyAPMConfig = new Dictionary<string, dynamic>
            {
                { "Enable", "true" },
                { "ServiceName", serviceName },
                { "Namespace", string.Empty },
                { "HeaderVersions", HeaderVersionsConfig },
                { "Sampling", samplingConfig },
                { "Logging", loggingConfig },
                { "MeterActive", true },
                { "MetricActive", true },
                { "SegmentActive", true },
                { "ProfilingActive", true },
                { "ManagementActive", true },
                { "LogActive", true },
                { "Transport", transportConfig }
            };

            var rootConfig = new Dictionary<string, dynamic>
            {
                { "SkyWalking", skyAPMConfig }
            };

            using (var writer = configFile.CreateText())
                writer.Write(JsonConvert.SerializeObject(rootConfig, Formatting.Indented));

            Console.WriteLine("Generate config file to {0}", configFilePath);
        }
    }
}
