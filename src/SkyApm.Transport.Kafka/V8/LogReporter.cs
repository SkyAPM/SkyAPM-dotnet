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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;
using Google.Protobuf;
using SkyApm.Config;
using SkyApm.Logging;
using SkyWalking.NetworkProtocol.V3;

namespace SkyApm.Transport.Kafka.V8
{
    public class LogReporter : ILogReporter
    {
        private readonly ILogger _logger;
        private readonly InstrumentConfig _instrumentConfig;
        private readonly KafkaConfig _config;
        private readonly ProducerConfig _producerConfig;
        private readonly ProducerBuilder<string, byte[]> _producerBuilder;
        private readonly IProducer<string, byte[]> _producer;
        private readonly string _topic;

        public LogReporter(ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(LogReporter));
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _config = configAccessor.Get<KafkaConfig>();
            _producerConfig = new ProducerConfig();
            _producerConfig.BootstrapServers = _config.BootstrapServers;
            _producerConfig.MessageTimeoutMs = _config.MessageTimeoutMs;
            _producerBuilder = new ProducerBuilder<string, byte[]>(_producerConfig);
            _producer = _producerBuilder.Build();
            _topic = _config.TopicLogs;
        }
        
        public async Task ReportAsync(IReadOnlyCollection<LogRequest> logRequests,
            CancellationToken cancellationToken = default)
        {
            // TODO
            // check whether _producer is okay?

            try
            {
                var stopwatch = Stopwatch.StartNew();
                foreach (var logRequest in logRequests)
                {
                    var logBody = new LogData()
                    {
                        Timestamp = logRequest.Date,
                        Service = _instrumentConfig.ServiceName,
                        ServiceInstance = _instrumentConfig.ServiceInstanceName,
                        Endpoint = logRequest.Endpoint ?? "null",
                        Body = new LogDataBody()
                        {
                            Type = "text",
                            Text = new TextLog()
                            {
                                Text = logRequest.Message,
                            },
                        },
                        Tags = new LogTags(),
                    };
                    if (logRequest.SegmentReference != null)
                    {
                        logBody.TraceContext = new TraceContext()
                        {
                            TraceId = logRequest.SegmentReference.TraceId,
                            TraceSegmentId = logRequest.SegmentReference.SegmentId,
                        };
                    }
                    foreach (var tag in logRequest.Tags)
                    {
                        logBody.Tags.Data.Add(new KeyStringValuePair()
                        {
                            Key = tag.Key,
                            Value = tag.Value.ToString(),
                        });
                    }
                    byte[] byteArray = logBody.ToByteArray();
                    await _producer.ProduceAsync(_topic, new Message<string, byte[]> { Key = logBody.Service, Value = byteArray });
                }
                stopwatch.Stop();
                _logger.Information($"Report {logRequests.Count} logs. cost: {stopwatch.Elapsed}s");
            }
            catch (IOException ex)
            {
                _logger.Error("Report log fail.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Report log fail.", ex);
            }
        }
    }
}
