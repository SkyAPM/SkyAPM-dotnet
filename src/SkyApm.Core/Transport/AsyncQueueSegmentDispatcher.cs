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
        private readonly ConcurrentQueue<SegmentRequest> _segmentQueue;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly CancellationTokenSource _cancellation;

        public AsyncQueueSegmentDispatcher(IConfigAccessor configAccessor,
            ISegmentReporter segmentReporter, IRuntimeEnvironment runtimeEnvironment,
            ISegmentContextMapper segmentContextMapper, ILoggerFactory loggerFactory)
        {
            _segmentReporter = segmentReporter;
            _segmentContextMapper = segmentContextMapper;
            _runtimeEnvironment = runtimeEnvironment;
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueSegmentDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _segmentQueue = new ConcurrentQueue<SegmentRequest>();
            _cancellation = new CancellationTokenSource();
        }

        public bool Dispatch(SegmentContext segmentContext)
        {
            if (!_runtimeEnvironment.Initialized || segmentContext == null || !segmentContext.Sampled)
                return false;

            if (_config.QueueSize < _segmentQueue.Count || _cancellation.IsCancellationRequested)
                return false;

            var segment = _segmentContextMapper.Map(segmentContext);

            if (segment == null)
                return false;

            _segmentQueue.Enqueue(segment);

            _logger.Debug($"Dispatch trace segment. [SegmentId]={segmentContext.SegmentId}.");
            return true;
        }

        public Task Flush(CancellationToken token = default(CancellationToken))
        {
            int batchSize = _config.BatchSize;
            int parallel = _config.Parallel;
            int pause = _config.Pause;
            bool flag = true;
            while (flag)
            {
                var tasks = new List<Task>(parallel);
                for (int i = 0; i < parallel; ++ i)
                {
                    var segments = new List<SegmentRequest>(batchSize);
                    for (int j = 0; j < batchSize; ++ j)
                    {
                        if (!_segmentQueue.TryDequeue(out var request))
                        {
                            flag = false;
                            break;
                        }
                        segments.Add(request);
                    }
                    if (segments.Count > 0)
                    {
                        Task task = _segmentReporter.ReportAsync(segments, token);
                        tasks.Add(task);
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
                if (tasks.Count > 0)
                {
                    try
                    {
                        // pause for a litte while
                        Task.WaitAll(tasks.ToArray(), TimeSpan.FromMilliseconds(pause));
                    }
                    catch (Exception e)
                    {
                        _logger.Debug("Task.WaitAll failed." + parallel + "," + pause);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public void Close()
        {
            _cancellation.Cancel();
        }
    }
}
