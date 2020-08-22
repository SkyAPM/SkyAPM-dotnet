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
    public class SegmentContext
    {
        public string SegmentId { get; }

        public string TraceId { get; }

        public SegmentSpan Span { get; }

        public string ServiceId { get; }

        public string ServiceInstanceId { get; }

        public bool Sampled { get; }

        public bool IsSizeLimited { get; } = false;

        public SegmentReferenceCollection References { get; } = new SegmentReferenceCollection();

        public SegmentContext(string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId,
            string operationName, SpanType spanType)
        {
            TraceId = traceId;
            Sampled = sampled;
            SegmentId = segmentId;
            ServiceId = serviceId;
            ServiceInstanceId = serviceInstanceId;
            Span = new SegmentSpan(operationName, spanType);
        }
    }
}