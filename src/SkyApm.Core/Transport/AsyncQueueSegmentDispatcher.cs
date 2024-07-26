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
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class AsyncQueueSegmentDispatcher : ISegmentDispatcher
    {
        private readonly ILogger _logger;
        private readonly TransportConfig _config;
        private readonly ISegmentReporter _segmentReporter;
        private readonly ISegmentContextMapper _segmentContextMapper;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly CancellationTokenSource _cancellation;
        private readonly Random _random;
        private long _dropCount = 0L;
        private long _produceCount = 0L;
        private long _consumeCount = 0L;
        private readonly BlockingCollection<SegmentRequest>[] _queueArray;
        private readonly long[] _countArray;

        public AsyncQueueSegmentDispatcher(IConfigAccessor configAccessor,
            ISegmentReporter segmentReporter, IRuntimeEnvironment runtimeEnvironment,
            ISegmentContextMapper segmentContextMapper, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueSegmentDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _segmentReporter = segmentReporter;
            _segmentContextMapper = segmentContextMapper;
            _runtimeEnvironment = runtimeEnvironment;
            _cancellation = new CancellationTokenSource();
            _random = new Random();
            _queueArray = new BlockingCollection<SegmentRequest>[_config.Parallel];
            _countArray = new long[_config.Parallel];
            for (int i = 0; i < _config.Parallel; ++ i)
            {
                _queueArray[i] = new BlockingCollection<SegmentRequest>(_config.QueueSize);
                _countArray[i] = 0;
            }
            for (int i = 0; i < _config.Parallel; ++ i)
            {
                int taskId = i;
                Task.Run(() => Flush(taskId));
            }
            Task.Run(() => Statistics());
        }

        public bool Dispatch(SegmentContext segmentContext)
        {
            if (!_runtimeEnvironment.Initialized || segmentContext == null || !segmentContext.Sampled)
                return false;

            if (_cancellation.IsCancellationRequested)
                return false;

            var segment = _segmentContextMapper.Map(segmentContext);

            if (segment == null)
                return false;

            int queueId = _random.Next(_config.Parallel);

            bool result = _queueArray[queueId].TryAdd(segment, 0);

            if (result)
            {
                Interlocked.Add(ref _produceCount, 1);
                Interlocked.Add(ref _countArray[queueId], 1);
            }
            else
            {
                Interlocked.Add(ref _dropCount, 1);
            }

            _logger.Debug($"Dispatch trace segment. [SegmentId]={segmentContext.SegmentId},[result=]{result}.");

            return result;
        }

        private void Flush(int taskId)
        {
            _logger.Information(
                "Flush." +
                "threadId=" + Thread.CurrentThread.ManagedThreadId + "," +
                "threadName=" + Thread.CurrentThread.Name + "," +
                taskId + ",");
            while (!_cancellation.IsCancellationRequested)
            {
                // handle dedicated queue
                {
                    int count = DoFlush(taskId, taskId, 2000);
                    if (count > 0)
                    {
                        continue;
                    }
                }
                // handle other queue
                {
                    int queueId = _random.Next(_config.Parallel);
                    if (queueId == taskId)
                    {
                        continue;
                    }
                    DoFlush(taskId, queueId, 0);
                }
            }
        }

        private int DoFlush(int taskId, int queueId, int timeout)
        {
            var segments = new List<SegmentRequest>(_config.BatchSize);
            for (int i = 0; i < _config.BatchSize; ++ i)
            {
                if (!_queueArray[queueId].TryTake(out var request, timeout))
                {
                    // segments is not full
                    break;
                }
                segments.Add(request);
            }
            if (segments.Count > 0)
            {
                try
                {
                    Task[] task = new Task[1];
                    task[0] = _segmentReporter.ReportAsync(segments, new CancellationToken());
                    bool result = Task.WaitAll(task, _config.Interval);
                    if (!result)
                    {
                        _logger.Warning(
                            "Task.WaitAll failed." +
                            "threadId=" + Thread.CurrentThread.ManagedThreadId + "," +
                            "threadName=" + Thread.CurrentThread.Name + "," +
                            "taskId=" + taskId + "," +
                            "queueId=" + queueId + "," +
                            "count=" + segments.Count + "," +
                            "timeout=" + _config.Interval + ",");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(
                        "Task.WaitAll failed." +
                        "threadId=" + Thread.CurrentThread.ManagedThreadId + "," +
                        "threadName=" + Thread.CurrentThread.Name + "," +
                        "taskId=" + taskId + "," +
                        "queueId=" + queueId + "," +
                        "count=" + segments.Count + ",",
                        e);
                }
                Interlocked.Add(ref _consumeCount, segments.Count);
                Interlocked.Add(ref _countArray[queueId], 0 - segments.Count);
            }
            return segments.Count;
        }

        public void Close()
        {
            _cancellation.Cancel();
        }

        private void Statistics()
        {
            _logger.Information("Statistics." + 
                "threadId=" + Thread.CurrentThread.ManagedThreadId + "," +
                "threadName=" + Thread.CurrentThread.Name + ",");
            while (!_cancellation.IsCancellationRequested)
            {
                _logger.Information(
                    "statistics." +
                    "threadId=" + Thread.CurrentThread.ManagedThreadId + "," +
                    "threadName=" + Thread.CurrentThread.Name + "," +
                    "drop=" + _dropCount + "," +
                    "produce=" + _produceCount + "," +
                    "consume=" + _consumeCount + "," +
                    "detail=[" + String.Join(",", _countArray) + "],");
                Thread.Sleep(1000 * 60);
            }
        }
    }
}
