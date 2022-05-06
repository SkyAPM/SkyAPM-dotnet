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

namespace SkyApm.Tracing.Segments
{
    public static class SkySegmentsExtensions
    {
        public static CrossThreadCarrier GetCrossThreadCarrier(this SegmentSpan span)
        {
            if (span == null) return null;

            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = span.Segment.TraceId,
                ParentSegmentId = span.Segment.SegmentId,
                ParentSpanId = span.SpanId,
                ParentServiceId = span.Segment.ServiceId,
                ParentServiceInstanceId = span.Segment.ServiceInstanceId,
                ParentEndpoint = span.Segment.FirstSpan.OperationName,
                Sampled = span.Segment.Sampled,
                NetworkAddress = DnsHelpers.GetIpV4OrHostName()
            };
        }

        public static CrossThreadCarrier GetCrossThreadCarrier(this SegmentContext segmentContext)
        {
            if (segmentContext == null) return null;

            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = segmentContext.TraceId,
                ParentSegmentId = segmentContext.SegmentId,
                ParentSpanId = segmentContext.Span.SpanId,
                ParentServiceId = segmentContext.ServiceId,
                ParentServiceInstanceId = segmentContext.ServiceInstanceId,
                ParentEndpoint = segmentContext.Span.OperationName,
                Sampled = segmentContext.Sampled,
                NetworkAddress = DnsHelpers.GetIpV4OrHostName()
            };
        }

        public static CrossThreadCarrier GetCrossThreadCarrier(this SpanOrSegmentContext spanOrSegmentContext)
        {
            if (spanOrSegmentContext == null) return null;

            return spanOrSegmentContext.SegmentContext == null ?
                GetCrossThreadCarrier(spanOrSegmentContext.SegmentSpan) :
                GetCrossThreadCarrier(spanOrSegmentContext.SegmentContext);
        }
    }
}
