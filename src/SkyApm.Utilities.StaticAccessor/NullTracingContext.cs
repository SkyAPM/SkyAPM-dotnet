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

using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class NullTracingContext : ITracingContext
    {
        public SegmentSpan ActiveSpan => NullInstances.SegmentSpan;

        public SpanOrSegmentContext CurrentEntry => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentLocal => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentExit => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentContext CreateLocalSegmentContext(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public void Finish(SpanOrSegmentContext spanOrSegmentContext)
        {
            
        }

        public void Release(SegmentContext segmentContext, long endTimeMilliseconds = 0)
        {
            
        }

        public void StopSpan(SegmentSpan span)
        {
            
        }

        public void StopSpan()
        {
            
        }
    }
}
