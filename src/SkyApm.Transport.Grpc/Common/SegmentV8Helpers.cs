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
using System.Linq;
using Google.Protobuf;
using SkyApm.Common;
using SkyWalking.NetworkProtocol.V3;

namespace SkyApm.Transport.Grpc.Common
{
    internal static class SegmentV8Helpers
    {
        public static SegmentObject Map(SegmentRequest request)
        {
            var traceSegment = new SegmentObject
            {
                TraceId = request.TraceId, //todo: is there chances request.UniqueIds.Count > 1 ?
                TraceSegmentId = request.Segment.SegmentId,
                Service = request.Segment.ServiceId,
                ServiceInstance = request.Segment.ServiceInstanceId,
                IsSizeLimited = false
            };

            traceSegment.Spans.Add(request.Segment.Spans.Select(MapToSpan).ToArray());
            return traceSegment;
        }

        private static SpanObject MapToSpan(SpanRequest request)
        {
            var spanObject = new SpanObject
            {
                SpanId = request.SpanId,
                ParentSpanId = request.ParentSpanId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SpanType = (SpanType) request.SpanType,
                SpanLayer = (SpanLayer) request.SpanLayer,
                IsError = request.IsError,
            };

            ReadStringOrIntValue(spanObject, request.Component, ComponentReader, ComponentIdReader);
            ReadStringOrIntValue(spanObject, request.OperationName, OperationNameReader, OperationNameIdReader);
            ReadStringOrIntValue(spanObject, request.Peer, PeerReader, PeerIdReader);

            spanObject.Tags.Add(request.Tags.Select(x => new KeyStringValuePair {Key = x.Key, Value = x.Value ?? string.Empty}));
            spanObject.Refs.AddRange(request.References.Select(MapToSegmentReference).ToArray());
            spanObject.Logs.AddRange(request.Logs.Select(MapToLogMessage).ToArray());

            return spanObject;
        }

        private static SegmentReference MapToSegmentReference(SegmentReferenceRequest referenceRequest)
        {
            var reference = new SegmentReference
            {
                TraceId = referenceRequest.TraceId, 
                ParentService = referenceRequest.ParentServiceId, 
                ParentServiceInstance = referenceRequest.ParentServiceInstanceId,
                ParentSpanId = referenceRequest.ParentSpanId,
                RefType = (RefType) referenceRequest.RefType,
                ParentTraceSegmentId = referenceRequest.ParentSegmentId,
                ParentEndpoint = referenceRequest.ParentEndpointName.ToString(),
                NetworkAddressUsedAtPeer = referenceRequest.NetworkAddress.ToString()
            };

            return reference;
        }

        private static Log MapToLogMessage(LogDataRequest request)
        {
            var logMessage = new Log {Time = request.Timestamp};
            logMessage.Data.AddRange(request.Data.Select(x => new KeyStringValuePair {Key = x.Key, Value = x.Value ?? string.Empty})
                .ToArray());
            return logMessage;
        }

        private static void ReadStringOrIntValue<T>(T instance, StringOrIntValue stringOrIntValue,
            Action<T, string> stringValueReader, Action<T, int> intValueReader)
        {
            // We should first check and prefer the int value to reduce the network transport payload 
            // in case both int and string value is present.
            if (stringOrIntValue.HasIntValue)
            {
                intValueReader.Invoke(instance, stringOrIntValue.GetIntValue());
            }
            else if (stringOrIntValue.HasStringValue)
            {
                stringValueReader.Invoke(instance, stringOrIntValue.GetStringValue());
            }
        }

        private static readonly Action<SpanObject, string> ComponentReader = (s, val) => { /*nolonger support*/};
        private static readonly Action<SpanObject, int> ComponentIdReader = (s, val) => s.ComponentId = val;
        private static readonly Action<SpanObject, string> OperationNameReader = (s, val) => s.OperationName = val;
        private static readonly Action<SpanObject, int> OperationNameIdReader = (s, val) => { /*nolonger support*/ };
        private static readonly Action<SpanObject, string> PeerReader = (s, val) => s.Peer = val;
        private static readonly Action<SpanObject, int> PeerIdReader = (s, val) => { /*nolonger support*/ };
    }
}