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

using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    public interface ITracingContext
    {
        SegmentSpan ActiveSpan { get; }

        TraceSegment ActiveSegment { get; }

        SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = default);

        SegmentContext CreateExitSegmentContext(string operationName, string networkAddress,
            ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        void Release(SegmentContext segmentContext, long endTimeMilliseconds = default);

        SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = default);

        SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        void StopSpan(SegmentSpan span);

        void StopSpan();

        CrossThreadCarrier StopSpanGetCarrier(SegmentSpan span);

        CrossThreadCarrier StopSpanGetCarrier();
    }
}