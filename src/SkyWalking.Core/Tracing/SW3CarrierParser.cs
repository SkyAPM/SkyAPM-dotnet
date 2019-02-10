/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The OpenSkywalking licenses this file to You under the Apache License, Version 2.0
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
using SkyWalking.Tracing.Segments;
using SkyWalking.Utils;

namespace SkyWalking.Tracing
{
    public class SW3CarrierParser : ICarrierParser
    {
        private const string HEADER = "sw3";
        private readonly IUniqueIdParser _uniqueIdParser;

        public SW3CarrierParser(IUniqueIdParser uniqueIdParser)
        {
            _uniqueIdParser = uniqueIdParser;
        }

        public bool TryParse(string key, string content, out ICarrier carrier)
        {
            bool defer(out ICarrier c)
            {
                c = new NullableCarrier();
                return false;
            }

            if (HEADER != key || string.IsNullOrEmpty(content))
                return defer(out carrier);

            var parts = content.Split("|".ToCharArray(), 8);
            if (parts.Length < 8)
                return defer(out carrier);

            if (!_uniqueIdParser.TryParse(parts[0], out var segmentId))
                return defer(out carrier);

            if (!int.TryParse(parts[1], out var parentSpanId))
                return defer(out carrier);

            if (!int.TryParse(parts[2], out var parentServiceInstanceId))
                return defer(out carrier);

            if (!int.TryParse(parts[3], out var entryServiceInstanceId))
                return defer(out carrier);

            if (!_uniqueIdParser.TryParse(parts[7], out var traceId))
                return defer(out carrier);

            carrier = new Carrier(traceId, segmentId, parentSpanId, parentServiceInstanceId,
                entryServiceInstanceId)
            {
                NetworkAddress = StringOrIntValueHelpers.ParseStringOrIntValue(parts[4]),
                EntryEndpoint = StringOrIntValueHelpers.ParseStringOrIntValue(parts[5]),
                ParentEndpoint = StringOrIntValueHelpers.ParseStringOrIntValue(parts[6])
            };

            return true;
        }

        public bool TryParse(string key, SegmentContext segmentContext, out ICarrier carrier)
        {
            if (HEADER != key)
            {
                carrier = new NullableCarrier();
                return false;
            }

            var reference = segmentContext.References.FirstOrDefault();

            carrier = new Carrier(segmentContext.TraceId, segmentContext.SegmentId, segmentContext.Span.SpanId,
                segmentContext.ServiceInstanceId, reference?.EntryServiceInstanceId ?? segmentContext.ServiceInstanceId)
            {
                NetworkAddress = segmentContext.Span.Peer,
                EntryEndpoint = reference?.EntryEndpoint ?? segmentContext.Span.OperationName,
                ParentEndpoint = segmentContext.Span.OperationName
            };

            return true;
        }
    }
}