/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Config;
using SkyWalking.Logging;

namespace SkyWalking.Transport
{
    public class BlockingTraceDispatcher : ITraceDispatcher
    {
        private readonly ILogger _logger;
        private readonly TransportConfig _config;
        private readonly BlockingCollection<TraceSegmentRequest> _limitCollection;
        private readonly IInstrumentationClient _instrumentationClient;
        private readonly ConcurrentQueue<TraceSegmentRequest> _segmentQueue;
        private readonly Task _consumer;
        private readonly int _queueTimeout;

        public BlockingTraceDispatcher(IConfigAccessor configAccessor, IInstrumentationClient client,ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(BlockingTraceDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _instrumentationClient = client;
            _segmentQueue = new ConcurrentQueue<TraceSegmentRequest>();
            _limitCollection = new BlockingCollection<TraceSegmentRequest>(_segmentQueue, _config.PendingSegmentLimit);
            _queueTimeout = _config.PendingSegmentTimeout;
        }

        public bool Dispatch(TraceSegmentRequest segment)
        {
            if (_limitCollection.IsAddingCompleted)
            {
                return false;
            }

            var result = _limitCollection.TryAdd(segment);

            if (result)
            {
                _logger.Debug($"Dispatch trace segment. [SegmentId]={segment.Segment.SegmentId}.");
            }
            
            return result;
        }

        public Task Flush(CancellationToken token = default(CancellationToken))
        {
            var limit = _config.PendingSegmentLimit;
            var index = 0;
            var segments = new List<TraceSegmentRequest>(limit);
            while (index++ < limit && _segmentQueue.TryDequeue(out var request))
            {
                segments.Add(request);
            }

            // send async
            _instrumentationClient.CollectAsync(segments, token);
            return Task.CompletedTask;
        }

        public void Close()
        {
            _limitCollection.CompleteAdding();
        }
    }
}