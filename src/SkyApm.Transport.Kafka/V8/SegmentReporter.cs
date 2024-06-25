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
using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;
using Google.Protobuf;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Kafka.Common;
using SkyWalking.NetworkProtocol.V3;

namespace SkyApm.Transport.Kafka.V8
{
    internal class SegmentReporter : ISegmentReporter
    {
        private readonly ILogger _logger;
        private readonly InstrumentConfig _instrumentConfig;
        private readonly TransportConfig _transportConfig;
        private readonly KafkaConfig _kafkaConfig;
        private readonly ProducerConfig _producerConfig;
        private readonly ProducerBuilder<string, byte[]> _producerBuilder;
        private readonly IProducer<string, byte[]> _producer;
        private readonly string _topic;

        public SegmentReporter(ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(SegmentReporter));
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _transportConfig = configAccessor.Get<TransportConfig>();
            _kafkaConfig = configAccessor.Get<KafkaConfig>();
            _producerConfig = new ProducerConfig();
            _producerConfig.BootstrapServers = _kafkaConfig.BootstrapServers;
            _producerConfig.MessageTimeoutMs = _kafkaConfig.MessageTimeoutMs;
            _producerConfig.QueueBufferingMaxMessages = 200000;
            // _producerConfig.QueueBufferingMaxKbytes = 1024 * 1024;
            _producerBuilder = new ProducerBuilder<string, byte[]>(_producerConfig);
            _producer = _producerBuilder.Build();
            _topic = _kafkaConfig.TopicSegments;
        }

        public async Task ReportAsync(IReadOnlyCollection<SegmentRequest> segmentRequests,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            // check whether producer is okay?

            long timestamp = DateTime.Now.Ticks;
            CountdownEvent countdownEvent = new CountdownEvent(segmentRequests.Count);
            int totalCount = segmentRequests.Count;
            int errorCount = 0;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                {
                    foreach (var segment in segmentRequests)
                    {
                        SegmentObject segmentObject = SegmentV8Helpers.Map(segment);
                        byte[] byteArray = segmentObject.ToByteArray();
                        _producer.Produce(
                            _topic,
                            new Message<string, byte[]> { Key = segmentObject.TraceSegmentId, Value = byteArray },
                            (DeliveryReport<string, byte[]> deliveryReport) =>
                            {
                                try
                                {
                                    countdownEvent.Signal();
                                }
                                catch (Exception e)
                                {
                                    _logger.Debug("countdownEvent.Signal failed." + e.ToString());
                                }
                                if (deliveryReport.Error.IsError)
                                {
                                    Interlocked.Add(ref errorCount, 1);
                                }
                                int remain = Interlocked.Add(ref totalCount, -1);
                                if (remain == 0)
                                {
                                    _logger.Information(
                                        "complete." +
                                        "totalCount=" + segmentRequests.Count + "," +
                                        "errorCount=" + errorCount + "," +
                                        "cost=" + (DateTime.Now.Ticks - timestamp) + ",");
                                }
                            });
                    }
                    bool result = countdownEvent.Wait(_transportConfig.Interval);
                    if (!result)
                    {
                        _logger.Warning(
                            "countdownEvent.Wait failed." +
                            "count=" + segmentRequests.Count + "," +
                            "timeout=" + _transportConfig.Interval + ",");
                    }
                }
                stopwatch.Stop();
                _logger.Information($"Report {segmentRequests.Count} trace segment. cost: {stopwatch.Elapsed}s");
            }
            catch (Exception ex)
            {
                _logger.Error("Report trace segment fail.", ex);
            }
            finally
            {
                countdownEvent.Dispose();
            }
        }
    }
}
