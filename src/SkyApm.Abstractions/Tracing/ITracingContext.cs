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
using System;

namespace SkyApm.Tracing
{
    public interface ITracingContext
    {
        #region SegmentContext
        [Obsolete("Use CreateEntry instead of this method")]
        SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        [Obsolete("Use CreateLocal instead of this method")]
        SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = default);

        [Obsolete("Use CreateLocal instead of this method")]
        SegmentContext CreateLocalSegmentContext(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        [Obsolete("Use CreateExit instead of this method")]
        SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        [Obsolete("Use CreateExit instead of this method")]
        SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        [Obsolete("Use Finish instead of this method")]
        void Release(SegmentContext segmentContext, long endTimeMilliseconds = default);
        #endregion SegmentContext

        #region SegmentSpan
        [Obsolete("Span structure only")]
        SegmentSpan ActiveSpan { get; }

        [Obsolete("Use CreateEntry instead of this method")]
        SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        [Obsolete("Use CreateLocal instead of this method")]
        SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = default);

        [Obsolete("Use CreateLocal instead of this method")]
        SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        [Obsolete("Use CreateExit instead of this method")]
        SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        [Obsolete("Use CreateExit instead of this method")]
        SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        [Obsolete("Use Finish instead of this method")]
        void StopSpan(SegmentSpan span);

        [Obsolete("Use Finish instead of this method")]
        void StopSpan();
        #endregion SegmentSpan

        #region SpanOrSegmentContext
        SpanOrSegmentContext CurrentEntry { get; }

        SpanOrSegmentContext CurrentLocal { get; }

        SpanOrSegmentContext CurrentExit { get; }

        SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        void Finish(SpanOrSegmentContext spanOrSegmentContext);
        #endregion SpanOrSegmentContext
    }
}