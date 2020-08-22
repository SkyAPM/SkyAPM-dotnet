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

using System.Collections.Generic;
using SkyApm.Common;

namespace SkyApm.Transport
{
    public class SegmentRequest
    {
        public string TraceId { get; set; }

        public SegmentObjectRequest Segment { get; set; }
    }

    public class SegmentObjectRequest
    {
        public string SegmentId { get; set; }

        public string ServiceId { get; set; }

        public string ServiceInstanceId { get; set; }

        public IList<SpanRequest> Spans { get; set; } = new List<SpanRequest>();
    }

    public class SpanRequest
    {
        public int SpanId { get; set; }

        public int SpanType { get; set; }

        public int SpanLayer { get; set; }

        public int ParentSpanId { get; set; }

        public long StartTime { get; set; }

        public long EndTime { get; set; }

        public StringOrIntValue Component { get; set; }

        public StringOrIntValue OperationName { get; set; }

        public StringOrIntValue Peer { get; set; }

        public bool IsError { get; set; }

        public IList<SegmentReferenceRequest> References { get; } = new List<SegmentReferenceRequest>();

        public IList<KeyValuePair<string, string>> Tags { get; } = new List<KeyValuePair<string, string>>();

        public IList<LogDataRequest> Logs { get; } = new List<LogDataRequest>();
    }

    public class SegmentReferenceRequest
    {
        public string TraceId { get; set; }

        public string ParentSegmentId { get; set; }

        public string ParentServiceId { get; set; }

        public string ParentServiceInstanceId { get; set; }

        public int ParentSpanId { get; set; }

        public string EntryServiceInstanceId { get; set; }

        public int RefType { get; set; }

        public StringOrIntValue ParentEndpointName { get; set; }

        public StringOrIntValue EntryEndpointName { get; set; }

        public StringOrIntValue NetworkAddress { get; set; }
    }

    public class LogDataRequest
    {
        public long Timestamp { get; set; }

        public IList<KeyValuePair<string, string>> Data { get; } = new List<KeyValuePair<string, string>>();
    }
}