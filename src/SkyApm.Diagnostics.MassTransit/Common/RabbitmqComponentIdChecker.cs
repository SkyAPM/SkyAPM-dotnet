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

using SkyApm.Common;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public class RabbitmqComponentIdChecker : IComponentIdChecker
    {
        public string PublishEndpointName => "rabbitmq";

        public string ConsumeEndpointName => PublishEndpointName;

        public StringOrIntValue CheckPublishComponentID(string host)
        {
            if (ContainsRabbitmq(host))
                return 52; //rabbitmq-producer 
            return 51; //rabbitmq
        }

        public StringOrIntValue CheckConsumeComponentID(string host)
        {
            if (ContainsRabbitmq(host))
                return 53; //rabbitmq-consumer 
            return 51; //rabbitmq
        }

        private bool ContainsRabbitmq(string host)
        {
            if (host.Contains("rabbitmq"))
                return true; // if url contains rabbitmq
            return false;
        }
    }
}
