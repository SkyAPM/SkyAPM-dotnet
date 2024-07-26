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
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Logging;
using SkyWalking.NetworkProtocol.V3;

namespace SkyApm.Transport.Kafka.V8
{
    internal class ServiceRegister : IServiceRegister
    {
        private const string OS_NAME = "os_name";
        private const string HOST_NAME = "host_name";
        private const string IPV4 = "ipv4";
        private const string PROCESS_NO = "process_no";
        private const string LANGUAGE = "language";
        private const int PROPERTIES_REPORT_PERIOD_FACTOR = 10;

        private readonly ILogger _logger;
        private readonly InstrumentConfig _instrumentConfig;
        private readonly KafkaConfig _config;
        private readonly ProducerConfig _producerConfig;
        private readonly ProducerBuilder<string, byte[]> _producerBuilder;
        private readonly IProducer<string, byte[]> _producer;
        private readonly string _topic;
        private readonly AtomicInteger _count;

        public ServiceRegister(ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(ServiceRegister));
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _config = configAccessor.Get<KafkaConfig>();
            _producerConfig = new ProducerConfig();
            _producerConfig.BootstrapServers = _config.BootstrapServers;
            _producerConfig.MessageTimeoutMs = _config.MessageTimeoutMs;
            _producerBuilder = new ProducerBuilder<string, byte[]>(_producerConfig);
            _producer = _producerBuilder.Build();
            _topic = _config.TopicManagements;
            _count = new AtomicInteger(0);
        }

        public async Task<bool> ReportInstancePropertiesAsync(ServiceInstancePropertiesRequest serviceInstancePropertiesRequest, CancellationToken cancellationToken = default)
        {
            // TODO
            // check whether producer is okay?

            int value = _count.Value;
            _count.Increment();

            try
            {
                if ((value % PROPERTIES_REPORT_PERIOD_FACTOR) == 0)
                {
                    var instance = new InstanceProperties
                    {
                        Service = serviceInstancePropertiesRequest.ServiceId,
                        ServiceInstance = serviceInstancePropertiesRequest.ServiceInstanceId,
                    };
                    instance.Properties.Add
                    (
                        new KeyStringValuePair
                        {
                            Key = OS_NAME,
                            Value = serviceInstancePropertiesRequest.Properties.OsName
                        }
                    );
                    instance.Properties.Add
                    (
                        new KeyStringValuePair
                        {
                            Key = HOST_NAME,
                            Value = serviceInstancePropertiesRequest.Properties.HostName
                        }
                    );
                    instance.Properties.Add
                    (
                        new KeyStringValuePair
                        {
                            Key = PROCESS_NO,
                            Value = serviceInstancePropertiesRequest.Properties.ProcessNo.ToString()
                        }
                    );
                    instance.Properties.Add
                    (
                        new KeyStringValuePair
                        {
                            Key = LANGUAGE,
                            Value = serviceInstancePropertiesRequest.Properties.Language
                        }
                    );
                    foreach (var ip in serviceInstancePropertiesRequest.Properties.IpAddress)
                    {
                        instance.Properties.Add
                        (
                            new KeyStringValuePair
                            {
                                Key = IPV4,
                                Value = ip
                            }
                        );
                    }

                    byte[] byteArray = instance.ToByteArray();
                    var result = await _producer.ProduceAsync(_topic, new Message<string, byte[]> { Key = "register-" + instance.ServiceInstance, Value = byteArray });
                }
                else
                {
                    var instance = new InstancePingPkg
                    {
                        Service = serviceInstancePropertiesRequest.ServiceId,
                        ServiceInstance = serviceInstancePropertiesRequest.ServiceInstanceId,
                    };

                    byte[] byteArray = instance.ToByteArray();
                    var result = await _producer.ProduceAsync(_topic, new Message<string, byte[]> { Key = instance.ServiceInstance, Value = byteArray });
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Report management fail.", e);
            }
            return false;
        }
    }
}
