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
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Config;

namespace SkyWalking.Transport
{
    public class BlockingTraceDispatcher : ITraceDispatcher
    {
        private readonly TransportConfig _config;
        private readonly BlockingCollection<TraceSegmentRequest> _limitCollection;
        private readonly IInstrumentationClient _instrumentationClient;
        private readonly ConcurrentQueue<TraceSegmentRequest> _segmentQueue;
        private readonly Task _consumer;
        private readonly CancellationTokenSource _consumerCancellation;

        public BlockingTraceDispatcher(IConfigAccessor configAccessor, IInstrumentationClient client)
        {
            _config = configAccessor.Get<TransportConfig>();
            _instrumentationClient = client;
            _limitCollection = new BlockingCollection<TraceSegmentRequest>(_config.PendingSegmentsLimit);
            _segmentQueue = new ConcurrentQueue<TraceSegmentRequest>();
            _consumerCancellation = new CancellationTokenSource();
            _consumer = Task.Factory.StartNew(
                Consumer, _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public bool Dispatch(TraceSegmentRequest segment)
        {
            if (_limitCollection.IsAddingCompleted)
            {
                return false;
            }

            _limitCollection.Add(segment);
            return true;
        }

        public Task Flush(CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            _limitCollection.CompleteAdding();
            _consumerCancellation.Cancel();
        }

        private void Consumer()
        {
            foreach (var consumingItem in _limitCollection.GetConsumingEnumerable(_consumerCancellation.Token))
            {
                _segmentQueue.Enqueue(consumingItem);
            }
        }
    }
}