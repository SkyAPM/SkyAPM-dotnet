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

using MassTransit;
using SkyApm.Common;
using System;
using System.Collections.Generic;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public class GetComponentUtil : IGetComponentUtil
    {
        private readonly IEnumerable<IComponentIdChecker> checkers;

        public GetComponentUtil(IEnumerable<IComponentIdChecker> checkers)
        {
            this.checkers = checkers;
        }
        public StringOrIntValue GetPublishComponentID<T>(T context) where T : SendContext
        {
            var host = context.DestinationAddress.AbsoluteUri;
            foreach (var checker in checkers)
            {
                if (host.Contains(checker.PublishEndpointName, StringComparison.OrdinalIgnoreCase))
                {
                    return checker.CheckPublishComponentID(host);
                }
            }
            return new StringOrIntValue(0, "Unknown");
        }

        public StringOrIntValue GetConsumeComponentID<T>(T context) where T : ConsumeContext
        {
            var host = context.SourceAddress.AbsoluteUri;
            foreach (var checker in checkers)
            {
                if (host.Contains(checker.ConsumeEndpointName, StringComparison.OrdinalIgnoreCase))
                {
                    return checker.CheckConsumeComponentID(host);
                }
            }
            return new StringOrIntValue(0, "Unknown");
        }
    }
}
