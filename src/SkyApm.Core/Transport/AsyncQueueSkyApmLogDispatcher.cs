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

using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Tracing.Segments;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport
{
    public class AsyncQueueSkyApmLogDispatcher : ISkyApmLogDispatcher
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellation;

        private readonly ConcurrentQueue<LoggerRequest> _segmentQueue;

        private readonly IRuntimeEnvironment _runtimeEnvironment;

        private readonly ILoggerReporter _loggerReporter;

        private readonly ILoggerContextContextMapper _loggerContextContextMapper;

        private readonly TransportConfig _config;

        private int _offset;

        public AsyncQueueSkyApmLogDispatcher(IConfigAccessor configAccessor, ILoggerFactory loggerFactory, ILoggerContextContextMapper loggerContextContextMapper, ILoggerReporter loggerReporter, IRuntimeEnvironment runtimeEnvironment)
        {
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueSkyApmLogDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _loggerContextContextMapper = loggerContextContextMapper;
            _runtimeEnvironment = runtimeEnvironment;
            _segmentQueue = new ConcurrentQueue<LoggerRequest>();
            _cancellation = new CancellationTokenSource();
            _loggerReporter= loggerReporter;
        }

        public bool Dispatch(LoggerContext loggerContext)
        {
            if (!_runtimeEnvironment.Initialized || loggerContext == null)
                return false;

            // todo performance optimization for ConcurrentQueue
            if (_config.QueueSize < _offset || _cancellation.IsCancellationRequested)
                return false;

            var segment = _loggerContextContextMapper.Map(loggerContext);

            if (segment == null)
                return false;

            _segmentQueue.Enqueue(segment);

            Interlocked.Increment(ref _offset);

            _logger.Debug($"Dispatch trace segment. [SegmentId]={loggerContext.SegmentContext.SegmentId}.");
            return true;
        }

        public Task Flush(CancellationToken token = default)
        {
            var limit = _config.BatchSize;
            var index = 0;
            var logges = new List<LoggerRequest>(limit);
            while (index++ < limit && _segmentQueue.TryDequeue(out var request))
            {
                logges.Add(request);
                Interlocked.Decrement(ref _offset);
            }

            // send async
            if (logges.Count > 0)
            {
                _loggerReporter.ReportAsync(logges, token);
            }

            Interlocked.Exchange(ref _offset, _segmentQueue.Count);

            return Task.CompletedTask;
        }
        public void Close()
        {
            _cancellation.Cancel();
        }
    }
}
