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

using SkyApm.Logging;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyApm.Tracing
{
    public class AsyncSpanCombiner : IAsyncSpanCombiner
    {
        private readonly ILogger _logger;

        public AsyncSpanCombiner(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(AsyncSpanCombiner));
        }

        public TraceSegment[] Merge(IEnumerable<TraceSegment> segments)
        {
            try
            {
                var mergedSegments = new Dictionary<string, TraceSegment>();

                var sortedSegments = segments.OrderBy(x => x.FirstSpan.AsyncDepth);
                foreach (var segment in sortedSegments)
                {
                    var reference = segment.FirstSpan.References.FirstOrDefault();
                    var parentSegmentId = reference?.ParentSegmentId;
                    if (parentSegmentId == null || !mergedSegments.TryGetValue(parentSegmentId, out var parentSegment))
                    {
                        mergedSegments.Add(segment.SegmentId, segment);
                        continue;
                    }

                    var hookSpan = parentSegment.Spans.First(x => x.SpanId == reference.ParentSpanId);
                    var sortedSpans = segment.Spans.OrderBy(x => x.SpanId);
                    foreach (var span in sortedSpans)
                    {
                        var asyncSpan = span == segment.FirstSpan ? hookSpan : parentSegment.Spans.FirstOrDefault(x => x.SpanId == span.SpanId);
                        if(asyncSpan == null)
                        {
                            if(span.Parent == segment.FirstSpan)
                            {
                                span.Parent = hookSpan;
                            }
                            parentSegment.Spans.Add(span);
                        }
                        else
                        {
                            asyncSpan.EndTime = span.EndTime;
                        }
                    }
                    mergedSegments.Add(segment.SegmentId, parentSegment);
                }

                var segmentArray = mergedSegments.Values.ToArray();
                TryResetAsyncDepth(segmentArray);
                return segmentArray;
            }
            catch(Exception e)
            {
                _logger.Error("merge async segments failed", e);
                return segments.ToArray();
            }
        }

        private void TryResetAsyncDepth(TraceSegment[] segments)
        {
            foreach (var segment in segments)
            {
                if(segment.FirstSpan.AsyncDepth == -1)
                {
                    foreach (var span in segment.Spans)
                    {
                        if(span.AsyncDepth != -1)
                        {
                            span.AsyncDepth = -1;
                            span.OperationName = $"[async]{span.OperationName}";
                        }
                    }
                }
            }
        }
    }
}
