/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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
using System.Collections.Generic;
using System.Text;
using SkyWalking.Dictionarys;
using SkyWalking.NetworkProtocol.Trace;

namespace SkyWalking.Context.Trace
{
    /// <summary>
    /// The <code>AbstractTracingSpan</code> represents a group of {@link ISpan} implementations, which belongs a real distributed trace.
    /// </summary>
    public abstract class AbstractTracingSpan : ISpan
    {
        protected int _spanId;
        protected int _parnetSpanId;
        protected Dictionary<string, string> _tags;
        protected string _operationName;
        protected int _operationId;
        protected SpanLayer _layer;

        /// <summary>
        /// The start time of this Span.
        /// </summary>
        protected long _startTime;

        /// <summary>
        /// The end time of this Span.
        /// </summary>
        protected long _endTime;

        protected bool _errorOccurred = false;

        protected int _componentId = 0;

        protected string _componentName;

        /// <summary>
        /// Log is a concept from OpenTracing spec. <p> {@see https://github.com/opentracing/specification/blob/master/specification.md#log-structured-data}
        /// </summary>
        protected ICollection<LogDataEntity> _logs;

        /// <summary>
        /// The refs of parent trace segments, except the primary one. For most RPC call, {@link #refs} contains only one
        /// element, but if this segment is a start span of batch process, the segment faces multi parents, at this moment,
        /// we use this {@link #refs} to link them.
        /// </summary>
        protected ICollection<ITraceSegmentRef> _refs;

        protected AbstractTracingSpan(int spanId,int parentSpanId,string operationName)
        {
            _operationName = operationName;
            _operationId = DictionaryUtil.NullValue;
            _spanId = spanId;
            _parnetSpanId = parentSpanId;
        }

        protected AbstractTracingSpan(int spanId,int parentSpanId,int operationId)
        {
            _operationName = null;
            _operationId = operationId;
            _spanId = spanId;
            _parnetSpanId = parentSpanId;
        }

        public virtual bool IsEntry => throw new NotImplementedException();

        public virtual bool IsExit => throw new NotImplementedException();

        public virtual int SpanId => throw new NotImplementedException();

        public virtual string OperationName => throw new NotImplementedException();

        public virtual int OperationId => throw new NotImplementedException();

        public virtual ISpan SetComponent(IComponent component)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan SetComponent(string componentName)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan Tag(string key, string value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, string>();
            }
            _tags.Add(key, value);
            return this;
        }

        public virtual ISpan SetLayer(SpanLayer layer)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan Log(Exception exception)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan ErrorOccurred()
        {
            throw new NotImplementedException();
        }

        public virtual ISpan Log(long timestamp, IDictionary<string, object> @event)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan SetOperationName(string operationName)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan Start()
        {
            throw new NotImplementedException();
        }

        public virtual ISpan SetOperationId(int operationId)
        {
            throw new NotImplementedException();
        }

        public virtual ISpan Start(long timestamp)
        {
            throw new NotImplementedException();
        }

        public virtual void Ref(ITraceSegmentRef traceSegmentRef)
        {
            throw new NotImplementedException();
        }

        //todo
        //public virtual bool Finish(ITraceSegmentRef)
    }
}
