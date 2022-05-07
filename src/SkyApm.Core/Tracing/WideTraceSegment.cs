using SkyApm.Tracing.Segments;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SkyApm.Tracing
{
    internal class WideTraceSegment : TraceSegment
    {
        private readonly AsyncLocal<int?> _spanTree = new AsyncLocal<int?>();
        private readonly ConcurrentDictionary<int, SegmentSpan> _spans = new ConcurrentDictionary<int, SegmentSpan>();
        private readonly ConcurrentDictionary<string, TraceSegment> _pathSegments = new ConcurrentDictionary<string, TraceSegment>();
        private readonly IUniqueIdGenerator _uniqueIdGenerator;
        private volatile int _spanId = -1;
        private readonly object _checkingLocker = new object();

        public SegmentSpan CurrentSpan
        {
            get
            {
                var spanId = _spanTree.Value;
                if (!spanId.HasValue) return null;

                return _spans.TryGetValue(spanId.Value, out var span) ? span : null;
            }
        }

        public WideTraceSegment(IUniqueIdGenerator uniqueIdGenerator, string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId)
            : base(traceId, segmentId, sampled, serviceId, serviceInstanceId)
        {
            _uniqueIdGenerator = uniqueIdGenerator;
        }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateEntrySpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();
            var spanType = spanId == 0 ? SpanType.Entry : SpanType.Local;

            return CreateSpan(spanId, operationName, spanType, startTimeMilliseconds);
        }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();

            return CreateSpan(spanId, operationName, SpanType.Local, startTimeMilliseconds);
        }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateExitSpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();

            return CreateSpan(spanId, operationName, SpanType.Exit, startTimeMilliseconds);
        }

        private SegmentSpan CreateSpan(int spanId, string operationName, SpanType spanType, long startTimeMilliseconds = default)
        {
            SegmentSpan parentSpan = null;
            if (_spanTree.Value.HasValue)
            {
                var parentSpanId = _spanTree.Value.Value;
                if (!_spans.TryGetValue(parentSpanId, out parentSpan)) return null;
            }

            var span = new SegmentSpan(operationName, spanType, startTimeMilliseconds);
            span.SpanId = spanId;
            if (parentSpan != null)
            {
                span.Parent = parentSpan;
                parentSpan.Children.TryAdd(spanId, span);
            }
            _spans.TryAdd(spanId, span);
            _spanTree.Value = spanId;
            if (FirstSpan == null) FirstSpan = span;

            return span;
        }

        private TraceSegment GetSegmentOwner(SegmentSpan span)
        {
            TraceSegment segment = null;
            var matchLength = 0;
            foreach (var item in _pathSegments)
            {
                if (span.SpanPath.StartsWith(item.Key) && (segment == null || item.Key.Length > matchLength))
                {
                    segment = item.Value;
                    matchLength = item.Key.Length;
                }
            }
            return segment ?? this;
        }

        public TraceSegment Finish(SegmentSpan span, long endTimeMilliseconds = default)
        {
            if (!_spans.TryGetValue(span.SpanId, out var storedSpan) || span != storedSpan) return null;
            if (!_spans.TryRemove(span.SpanId, out _)) return null;

            TraceSegment segment;
            lock (_checkingLocker)
            {// 使用锁时防止异步任务在子TraceSegment创建之后，_pathSegments新增之前完成，这种情况会导致异步任务span错误的追加到父级segment中
                span.Finish(endTimeMilliseconds);
                _spanTree.Value = span.Parent?.SpanId;
                segment = GetSegmentOwner(span);
                var children = span.Children.Values.ToArray();
                foreach (var child in children)
                {
                    // span完成时，检查所有子span是否已完成，如果未完成，那么子span为异步任务，新建segment并建立关联关系
                    if (child.EndTime == default)
                    {
                        var childSegment = new TraceSegment(TraceId, _uniqueIdGenerator.Generate(), Sampled, ServiceId, ServiceInstanceId);
                        childSegment.FirstSpan = child;
                        child.AsyncDepth = segment.FirstSpan.AsyncDepth;
                        AsyncDeepCopyAndUpdateSpans(child, span, segment.Spans, childSegment.Spans);
                        child.SpanType = SpanType.Local; // 实际的spanType已经在上面copy一份了，由于服务端对特殊span有额外的处理逻辑，为了避免重复处理，这里设置为local
                        var reference = new SegmentReference
                        {
                            Reference = Reference.CrossThread,
                            TraceId = segment.TraceId,
                            ParentSegmentId = segment.SegmentId,
                            ParentSpanId = child.SpanId,
                            ParentServiceId = segment.ServiceId,
                            ParentServiceInstanceId = segment.ServiceInstanceId,
                            ParentEndpoint = segment.FirstSpan.OperationName,
                            NetworkAddress = segment.FirstSpan.OperationName,
                            EntryEndpoint = segment.FirstSpan.OperationName,
                            EntryServiceInstanceId = segment.ServiceInstanceId
                        };
                        child.References.Add(reference);
                        _pathSegments.TryAdd(child.SpanPath, childSegment);
                    }
                }
            }

            segment.Spans.Add(span);
            return span == segment.FirstSpan ? segment : null;
        }

        public SegmentSpan AsyncDeepCopyAndUpdateSpans(SegmentSpan span, SegmentSpan parent, IList<SegmentSpan> spans, IList<SegmentSpan> newSegmentSpans)
        {
            if (span.EndTime != default) return span;

            var asyncDepth = span.AsyncDepth == -1 ? 0 : span.AsyncDepth;
            span.AsyncDepth = asyncDepth + 1;
            var copySpan = new SegmentSpan(span.OperationName.ToString(), span.SpanType, span.StartTime)
            {
                SpanId = span.SpanId,
                Peer = span.Peer,
                SpanLayer = span.SpanLayer,
                Component = span.Component,
                IsError = span.IsError,
                AsyncDepth = asyncDepth,
                Parent = parent
            };
            foreach (var tag in span.Tags)
            {
                copySpan.AddTag(tag.Key, tag.Value);
            }
            foreach (var log in span.Logs)
            {
                copySpan.AddLog(log);
            }
            foreach (var child in span.Children)
            {
                var copyChild = AsyncDeepCopyAndUpdateSpans(child.Value, copySpan, spans, newSegmentSpans);
                if (copyChild != null)
                {
                    copySpan.Children.TryAdd(child.Key, copyChild);
                }
            }

            copySpan.Finish();
            spans.Add(copySpan);

            return copySpan;
        }

        private int NextSpanId() => Interlocked.Increment(ref _spanId);
    }
}
