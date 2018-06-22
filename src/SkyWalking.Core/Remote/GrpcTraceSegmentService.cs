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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Boot;
using SkyWalking.Context;
using SkyWalking.Context.Trace;
using SkyWalking.Logging;
using SkyWalking.NetworkProtocol;
using SkyWalking.Utils;

namespace SkyWalking.Remote
{
    public class GrpcTraceSegmentService : TimerService, ITracingContextListener
    {
        private static readonly ILogger _logger = LogManager.GetLogger<GrpcTraceSegmentService>();
        private static readonly System.Collections.Concurrent.ConcurrentQueue<ITraceSegment> _traceSegments
            = new System.Collections.Concurrent.ConcurrentQueue<ITraceSegment>();
        private static readonly int _batchSize = 1000;

        public override void Dispose()
        {
            TracingContext.ListenerManager.Remove(this);
            if(_traceSegments.Count > 0)
            {
                Task.Run(() => BatchSendTraceSegments(true));
            }
            base.Dispose();
        }

        public override int Order { get; } = 1;

        protected override TimeSpan Interval => TimeSpan.FromSeconds(1);

        protected override Task Initializing(CancellationToken token)
        {
            base.Initializing(token);
            TracingContext.ListenerManager.Add(this);
            return TaskUtils.CompletedTask;
        }

        public async void AfterFinished(ITraceSegment traceSegment)
        {
            if (traceSegment.IsIgnore)
            {
                return;
            }

            _traceSegments.Enqueue(traceSegment);
        }

        protected async override Task Execute(CancellationToken token)
        {
            await BatchSendTraceSegments(false);
        }

        /// <summary>
        /// Batch send the queued trace segments to collector until the number of left segments 
        /// equals zero or lessthan batchsize denpends on the forceClear param
        /// </summary>
        /// <param name="forceClear">wether we should send all segments in the queue 
        /// or leave the last lessthan batchsize segments for the next batch send process</param>
        /// <returns></returns>
        private async Task BatchSendTraceSegments(bool forceClear)
        {
            do
            {
                if (_traceSegments.Count == 0)
                    return;

                var availableConnection = GrpcConnectionManager.Instance.GetAvailableConnection();
                if (availableConnection == null)
                {
                    _logger.Warning(
                        $"Transform and send UpstreamSegment to collector fail. {GrpcConnectionManager.NotFoundErrorMessage}");
                    return;
                }

                var segments = new List<ITraceSegment>();
                var i = 0;
                while ((_batchSize <= 0 || i++ < _batchSize) && _traceSegments.TryDequeue(out var segment))
                    segments.Add(segment);
                if (segments.Count == 0)
                    return;

                try
                {
                    var traceSegmentService =
                        new TraceSegmentService.TraceSegmentServiceClient(availableConnection.GrpcChannel);
                    using (var asyncClientStreamingCall = traceSegmentService.collect())
                    {
                        segments.ForEach(async segment => await asyncClientStreamingCall.RequestStream.WriteAsync(segment.Transform()));
                        await asyncClientStreamingCall.RequestStream.CompleteAsync();
                        await asyncClientStreamingCall.ResponseAsync;
                    }

                    _logger.Debug(
                            $"Transform and send UpstreamSegment to collector. [Total TraceSegment Count] = {segments.Count}");
                }
                catch (Exception e)
                {
                    _logger.Warning($"Transform and send UpstreamSegment to collector fail. {e.Message}");
                    availableConnection?.Failure();
                    return;
                }

            } while (forceClear || _traceSegments.Count >= _batchSize);
        }
    }
}