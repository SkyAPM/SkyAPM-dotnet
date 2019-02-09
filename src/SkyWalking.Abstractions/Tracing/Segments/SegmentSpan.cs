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

using System;
using System.Collections;
using System.Collections.Generic;

namespace SkyWalking.Tracing.Segments
{
    public class SegmentSpan
    {
        public int SpanId { get; } = 0;

        public int ParentSpanId { get; } = -1;

        public long StartTime { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public long EndTime { get; private set; }

        public StringOrIntValue OperationName { get; set; }

        public StringOrIntValue Peer { get; set; }

        public SpanType SpanType { get; set; }

        public SpanLayer SpanLayer { get; set; }

        public StringOrIntValue Component { get; set; }

        public bool IsError { get; set; }
        public TagCollection Tags { get; } = new TagCollection();

        public LogCollection Logs { get; } = new LogCollection();

        public SegmentSpan AddTag(string key, string value)
        {
            Tags.AddTag(key, value);
            return this;
        }

        public SegmentSpan AddTag(string key, long value)
        {
            Tags.AddTag(key, value.ToString());
            return this;
        }

        public SegmentSpan AddTag(string key, bool value)
        {
            Tags.AddTag(key, value.ToString());
            return this;
        }

        public SpanLog AddLog(string @event, string message)
        {
            var log = new SpanLog(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Logs.AddLog(log);
            return log;
        }

        public void Finish()
        {
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public class TagCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> tags = new Dictionary<string, string>();

        internal void AddTag(string key, string value)
        {
            tags[key] = value;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tags.GetEnumerator();
        }
    }

    public enum SpanType
    {
        Entry = 0,
        Exit = 1,
        Local = 2
    }

    public enum SpanLayer
    {
        DB = 1,
        RPC_FRAMEWORK = 2,
        HTTP = 3,
        MQ = 4,
        CACHE = 5
    }

    public class LogCollection : IEnumerable<SpanLog>
    {
        private readonly List<SpanLog> _logs = new List<SpanLog>();

        internal void AddLog(SpanLog log)
        {
            _logs.Add(log);
        }

        public IEnumerator<SpanLog> GetEnumerator()
        {
            return _logs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _logs.GetEnumerator();
        }
    }

    public class SpanLog
    {
        public long Timestamp { get; }

        public Dictionary<string, string> Data { get; }

        public SpanLog(long timestamp)
        {
            Timestamp = timestamp;
            Data = new Dictionary<string, string>();
        }
    }
}