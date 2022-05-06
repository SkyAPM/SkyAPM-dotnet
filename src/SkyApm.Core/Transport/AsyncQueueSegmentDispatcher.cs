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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class AsyncQueueSegmentDispatcher : ISegmentDispatcher
    {
        private readonly ILogger _logger;
        private readonly TransportConfig _config;
        private readonly SpanStructureConfig _spanConfig;
        private readonly ISegmentReporter _segmentReporter;
        private readonly ISegmentContextMapper _segmentContextMapper;
        private readonly IAsyncSpanCombiner _asyncSpanCombiner;
        private readonly ConcurrentQueue<SegmentRequest> _segmentQueue;
        private readonly ConcurrentQueue<TraceSegment> _mergeQueue;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TraceSegment>> _mergeDictionary;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly CancellationTokenSource _cancellation;
        private Task _mergeTask;
        private int _offset;
        private int _mergeCount;

        public AsyncQueueSegmentDispatcher(IConfigAccessor configAccessor, IAsyncSpanCombiner asasyncSpanCombiner,
            ISegmentReporter segmentReporter, IRuntimeEnvironment runtimeEnvironment,
            ISegmentContextMapper segmentContextMapper, ILoggerFactory loggerFactory)
        {
            _segmentReporter = segmentReporter;
            _segmentContextMapper = segmentContextMapper;
            _asyncSpanCombiner = asasyncSpanCombiner;
            _runtimeEnvironment = runtimeEnvironment;
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueSegmentDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _spanConfig = configAccessor.Get<SpanStructureConfig>();
            _segmentQueue = new ConcurrentQueue<SegmentRequest>();
            _mergeQueue = new ConcurrentQueue<TraceSegment>();
            _mergeDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, TraceSegment>>();
            _cancellation = new CancellationTokenSource();
            _mergeTask = Task.Factory.StartNew(() => _mergeTask = BackgroundMerge(), _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public bool Dispatch(SegmentContext segmentContext)
        {
            if (!_runtimeEnvironment.Initialized || segmentContext == null || !segmentContext.Sampled)
                return false;

            // todo performance optimization for ConcurrentQueue
            if (_config.QueueSize < _offset || _cancellation.IsCancellationRequested)
                return false;

            var segment = _segmentContextMapper.Map(segmentContext);

            if (segment == null)
                return false;

            Enqueue(segment);

            return true;
        }

        public bool Dispatch(TraceSegment traceSegment)
        {
            if (!_runtimeEnvironment.Initialized || traceSegment == null || !traceSegment.Sampled)
                return false;

            // todo performance optimization for ConcurrentQueue
            if (_config.QueueSize < _offset || _cancellation.IsCancellationRequested)
                return false;

            var segment = _segmentContextMapper.MapIfNoAsync(traceSegment);

            if (segment == null)
            {
                var mergeEnqueue = Merge(traceSegment);
                if (mergeEnqueue) return true;

                segment = _segmentContextMapper.Map(traceSegment);
            }

            Enqueue(segment);

            return true;
        }

        public Task Flush(CancellationToken token = default(CancellationToken))
        {
            // todo performance optimization for ConcurrentQueue
            //var queued = _segmentQueue.Count;
            //var limit = queued <= _config.PendingSegmentLimit ? queued : _config.PendingSegmentLimit;
            var limit = _config.BatchSize;
            var index = 0;
            var segments = new List<SegmentRequest>(limit);
            while (index++ < limit && _segmentQueue.TryDequeue(out var request))
            {
                segments.Add(request);
                Interlocked.Decrement(ref _offset);
            }

            // send async
            if (segments.Count > 0)
                _segmentReporter.ReportAsync(segments, token);

            Interlocked.Exchange(ref _offset, _segmentQueue.Count);

            return Task.CompletedTask;
        }

        public void Close()
        {
            _cancellation.Cancel();
        }

        private void Enqueue(SegmentRequest segment)
        {
            _segmentQueue.Enqueue(segment);

            Interlocked.Increment(ref _offset);

            _logger.Debug($"Dispatch trace segment. [SegmentId]={segment.Segment.SegmentId}.");
        }

        private bool Merge(TraceSegment segment)
        {
            var count = Interlocked.Increment(ref _mergeCount);
            if (_cancellation.IsCancellationRequested || count >= _spanConfig.MergeQueueSize ||
                segment.FirstSpan.EndTime + _spanConfig.MergeDelay <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                Interlocked.Decrement(ref _mergeCount);
                return false;
            }
            var segmentDictionary = _mergeDictionary.GetOrAdd(segment.TraceId, traceId =>
            {
                var dictionary = new ConcurrentDictionary<string, TraceSegment>();
                dictionary.TryAdd(segment.SegmentId, segment);
                return dictionary;
            });
            if (segmentDictionary.ContainsKey(segment.SegmentId))
            {
                _mergeQueue.Enqueue(segment);
            }
            else
            {
                segmentDictionary.TryAdd(segment.SegmentId, segment);
            }
            return true;
        }

        private async Task BackgroundMerge()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                if (!_mergeQueue.TryDequeue(out var segment))
                {
                    try
                    {
                        await Task.Delay(_spanConfig.MergeDelay, _cancellation.Token);
                        continue;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

                var deadline = segment.FirstSpan.EndTime + _spanConfig.MergeDelay;
                var current = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var delay = (int)(deadline - current);
                if (delay > 100)
                {
                    try
                    {
                        await Task.Delay(delay, _cancellation.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        _mergeQueue.Enqueue(segment);
                        break;
                    }
                }

                if (!_mergeDictionary.TryRemove(segment.TraceId, out var segments)) continue;

                MergeAndEnqueue(segments.Values);
            }

            foreach (var traceId in _mergeDictionary.Keys.ToArray())
            {
                if(_mergeDictionary.TryRemove(traceId, out var segments))
                {
                    MergeAndEnqueue(segments.Values);
                }
            }
        }

        private void MergeAndEnqueue(IEnumerable<TraceSegment> segments)
        {
            var mergedSegments = _asyncSpanCombiner.Merge(segments);
            foreach (var mergedSegment in mergedSegments)
            {
                if (mergedSegment == null) continue;

                var segmentRequest = _segmentContextMapper.Map(mergedSegment);

                Enqueue(segmentRequest);
            }
        }
    }
}