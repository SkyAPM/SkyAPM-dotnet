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
using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;
using Google.Protobuf;
using SkyApm.Config;
using SkyApm.Logging;
using SkyWalking.NetworkProtocol.V3;

namespace SkyApm.Transport.Kafka.V8
{
    internal class CLRStatsReporter : ICLRStatsReporter
    {
        private readonly ILogger _logger;
        private readonly InstrumentConfig _instrumentConfig;
        private readonly KafkaConfig _config;
        private readonly ProducerConfig _producerConfig;
        private readonly ProducerBuilder<string, byte[]> _producerBuilder;
        private readonly IProducer<string, byte[]> _producer;
        private readonly string _topic;

        public CLRStatsReporter(ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(CLRStatsReporter));
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _config = configAccessor.Get<KafkaConfig>();
            _producerConfig = new ProducerConfig();
            _producerConfig.BootstrapServers = _config.BootstrapServers;
            _producerConfig.MessageTimeoutMs = _config.MessageTimeoutMs;
            _producerBuilder = new ProducerBuilder<string, byte[]>(_producerConfig);
            _producer = _producerBuilder.Build();
            _topic = _config.TopicCLRMetrics;
        }

        public async Task ReportAsync(CLRStatsRequest statsRequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            // check whether _producer is okay

            try
            {
                var request = new CLRMetricCollection
                {
                    Service = _instrumentConfig.ServiceName,
                    ServiceInstance = _instrumentConfig.ServiceInstanceName,
                };
                var metric = new CLRMetric
                {
                    Cpu = new CPU
                    {
                        UsagePercent = statsRequest.CPU.UsagePercent
                    },
                    Gc = new ClrGC
                    {
                        Gen0CollectCount = statsRequest.GC.Gen0CollectCount,
                        Gen1CollectCount = statsRequest.GC.Gen1CollectCount,
                        Gen2CollectCount = statsRequest.GC.Gen2CollectCount,
                        HeapMemory = statsRequest.GC.HeapMemory
                    },
                    Thread = new ClrThread
                    {
                        AvailableWorkerThreads = statsRequest.Thread.AvailableWorkerThreads,
                        AvailableCompletionPortThreads = statsRequest.Thread.AvailableCompletionPortThreads,
                        MaxWorkerThreads = statsRequest.Thread.MaxWorkerThreads,
                        MaxCompletionPortThreads = statsRequest.Thread.MaxCompletionPortThreads
                    },
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                request.Metrics.Add(metric);
                byte[] byteArray = request.ToByteArray();
                await _producer.ProduceAsync(_topic, new Message<string, byte[]> { Key = request.ServiceInstance, Value = byteArray });
            }
            catch (Exception e)
            {
                _logger.Warning("Report CLR Stats error. " + e);
            }
        }
    }
}
