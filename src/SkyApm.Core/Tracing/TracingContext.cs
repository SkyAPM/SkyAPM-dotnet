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
using SkyApm.Common;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;

namespace SkyApm.Tracing
{
    public class TracingContext : ITracingContext
    {
        private readonly ISegmentContextFactory _segmentContextFactory;
        private readonly ITraceSegmentManager _traceSegmentManager;
        private readonly ICarrierPropagator _carrierPropagator;
        private readonly ISegmentDispatcher _segmentDispatcher;

        public TracingContext(ISegmentContextFactory segmentContextFactory,
            ITraceSegmentManager traceSegmentManager,
            ICarrierPropagator carrierPropagator,
            ISegmentDispatcher segmentDispatcher)
        {
            _segmentContextFactory = segmentContextFactory;
            _traceSegmentManager = traceSegmentManager;
            _carrierPropagator = carrierPropagator;
            _segmentDispatcher = segmentDispatcher;
        }
        public SegmentSpan ActiveSpan => _traceSegmentManager.ActiveSpan;

        public TraceSegment ActiveSegment => _traceSegmentManager.ActiveSegment;

        public SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            var carrier = _carrierPropagator.Extract(carrierHeader);
            return _segmentContextFactory.CreateEntrySegment(operationName, carrier, startTimeMilliseconds);
        }

        public SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _segmentContextFactory.CreateLocalSegment(operationName, startTimeMilliseconds);
        }

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress,
            ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            var segmentContext =
                _segmentContextFactory.CreateExitSegment(operationName, new StringOrIntValue(networkAddress), startTimeMilliseconds);
            if (carrierHeader != null)
                _carrierPropagator.Inject(segmentContext, carrierHeader);
            return segmentContext;
        }

        public void Release(SegmentContext segmentContext, long endTimeMilliseconds = default)
        {
            if (segmentContext == null)
            {
                return;
            }

            _segmentContextFactory.Release(segmentContext, endTimeMilliseconds);
            if (segmentContext.Sampled)
                _segmentDispatcher.Dispatch(segmentContext);
        }

        public SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            var carrier = _carrierPropagator.Extract(carrierHeader);
            return _traceSegmentManager.CreateEntrySpan(operationName, carrier, startTimeMilliseconds);
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _traceSegmentManager.CreateLocalSpan(operationName, startTimeMilliseconds);
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _traceSegmentManager.CreateLocalSpan(operationName, carrier, startTimeMilliseconds);
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            var span = _traceSegmentManager.CreateExitSpan(operationName, new StringOrIntValue(networkAddress), startTimeMilliseconds);
            var segment = _traceSegmentManager.ActiveSegment;
            if (carrierHeader != null) _carrierPropagator.Inject(segment, span, carrierHeader);
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            var span = _traceSegmentManager.CreateExitSpan(operationName, new StringOrIntValue(networkAddress), carrier, startTimeMilliseconds);
            var segment = _traceSegmentManager.ActiveSegment;
            if (carrierHeader != null) _carrierPropagator.Inject(segment, span, carrierHeader);
            return span;
        }

        public void StopSpan(SegmentSpan span)
        {
            var segment = _traceSegmentManager.StopSpan(span);

            if(segment != null && segment.Sampled)
            {
                segment.FirstSpan.SpanId = 0;
                _segmentDispatcher.Dispatch(segment);
            }
        }

        public void StopSpan()
        {
            (var segment, _) = _traceSegmentManager.StopSpan();

            if (segment != null && segment.Sampled)
            {
                _segmentDispatcher.Dispatch(segment);
            }
        }

        public CrossThreadCarrier StopSpanGetCarrier(SegmentSpan span)
        {
            var segment = _traceSegmentManager.StopSpan(span);

            if (segment != null && segment.Sampled)
            {
                _segmentDispatcher.Dispatch(segment);
                return segment.GetCrossThreadCarrier(span.SpanId);
            }

            return null;
        }

        public CrossThreadCarrier StopSpanGetCarrier()
        {
            (var segment, var span) = _traceSegmentManager.StopSpan();

            if (segment != null && segment.Sampled)
            {
                _segmentDispatcher.Dispatch(segment);
                return segment.GetCrossThreadCarrier(span.SpanId);
            }

            return null;
        }
    }
}