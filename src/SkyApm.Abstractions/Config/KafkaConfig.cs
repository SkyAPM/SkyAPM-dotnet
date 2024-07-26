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

namespace SkyApm.Config
{
    [Config("SkyWalking", "Transport", "Kafka")]
    public class KafkaConfig
    {
        ///
        /// <summary>
        /// e.g. address1:port1[,address2:port2...]
        /// </summary>
        ///
        public string BootstrapServers { get; set; }

        ///
        /// <summary>
        /// in milliseconds
        /// </summary>
        ///
        public int TopicTimeoutMs { get; set; } = 3000;

        ///
        /// <summary>
        /// in milliseconds
        /// </summary>
        ///
        public int MessageTimeoutMs { get; set; } = 5000;

        public string TopicMeters { get; set; }

        public string TopicCLRMetrics { get; set; }

        public string TopicSegments { get; set; }

        public string TopicProfilings { get; set; }

        public string TopicManagements { get; set; }

        public string TopicLogs { get; set; }
    }
}
