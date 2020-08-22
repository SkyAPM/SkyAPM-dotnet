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

using System.Linq;
using SkyApm.Common;
using SkyApm.Config;

namespace SkyApm.Tracing
{
    public class Sw8CarrierFormatter : ICarrierFormatter
    {
        private readonly IBase64Formatter _base64Formatter;

        public Sw8CarrierFormatter(IBase64Formatter base64Formatter,
            IConfigAccessor configAccessor)
        {
            _base64Formatter = base64Formatter;
            var config = configAccessor.Get<InstrumentConfig>();
            Key = string.IsNullOrEmpty(config.Namespace)
                ? HeaderVersions.SW8
                : $"{config.Namespace}-{HeaderVersions.SW8}";
            Enable = config.HeaderVersions == null || config.HeaderVersions.Contains(HeaderVersions.SW8);
        }

        public string Key { get; }

        public bool Enable { get; }

        public ICarrier Decode(string content)
        {
            NullableCarrier Defer()
            {
                return NullableCarrier.Instance;
            }

            if (string.IsNullOrEmpty(content))
                return Defer();

            var parts = content.Split('-');
            if (parts.Length < 8)
                return Defer();

            if (!int.TryParse(parts[0], out var sampled))
                return Defer();

            var traceId = _base64Formatter.Decode(parts[1]);
            var segmentId = _base64Formatter.Decode(parts[2]);

            if (!int.TryParse(parts[3], out var parentSpanId))
                return Defer();

            var parentService = _base64Formatter.Decode(parts[4]);
            var parentServiceInstance = _base64Formatter.Decode(parts[5]);
            var parentEndpoint = _base64Formatter.Decode(parts[6]);
            var networkAddress = _base64Formatter.Decode(parts[7]);

            var carrier = new Carrier(traceId, segmentId, parentSpanId, parentServiceInstance,
                default, parentService)
            {
                NetworkAddress = networkAddress,
                ParentEndpoint = parentEndpoint,
                Sampled = sampled != 0
            };

            return carrier;
        }

        public string Encode(ICarrier carrier)
        {
            if (!carrier.HasValue)
                return string.Empty;
            return string.Join("-",
                carrier.Sampled != null && carrier.Sampled.Value ? "1" : "0",
                _base64Formatter.Encode(carrier.TraceId),
                _base64Formatter.Encode(carrier.ParentSegmentId),
                carrier.ParentSpanId.ToString(),
                _base64Formatter.Encode(carrier.ParentServiceId),
                _base64Formatter.Encode(carrier.ParentServiceInstanceId),
                _base64Formatter.Encode(carrier.ParentEndpoint.ToString()),
                _base64Formatter.Encode(carrier.NetworkAddress.ToString()));
        }
    }
}