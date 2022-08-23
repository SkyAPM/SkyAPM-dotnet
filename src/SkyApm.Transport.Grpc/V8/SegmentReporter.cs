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

using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Grpc.Common;
using SkyWalking.NetworkProtocol.V3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc.V8
{
    internal class SegmentReporter : ISegmentReporter
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        public SegmentReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _config = configAccessor.Get<GrpcConfig>();
            _logger = loggerFactory.CreateLogger(typeof(SegmentReporter));
        }

        public async Task ReportAsync(IReadOnlyCollection<SegmentRequest> segmentRequests,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_connectionManager.Ready)
            {
                return;
            }

            var connection = _connectionManager.GetConnection();

            try
            {
                var requests = FilterSegmentRequests(segmentRequests);
                var stopwatch = Stopwatch.StartNew();
                var client = new TraceSegmentReportService.TraceSegmentReportServiceClient(connection);
                using (var asyncClientStreamingCall =
                    client.collect(_config.GetMeta(), _config.GetReportTimeout(), cancellationToken))
                {
                    foreach (var segment in requests)
                    {
                        await asyncClientStreamingCall.RequestStream.WriteAsync(SegmentV8Helpers.Map(segment));
                    }
                    await asyncClientStreamingCall.RequestStream.CompleteAsync();
                    await asyncClientStreamingCall.ResponseAsync;
                }
                stopwatch.Stop();
                _logger.Information($"Report {segmentRequests.Count} trace segment. cost: {stopwatch.Elapsed}s");
            }
            catch (Exception ex)
            {
                _logger.Error("Report trace segment fail.", ex);
                _connectionManager.Failure(ex);
            }
        }

        private IEnumerable<SegmentRequest> FilterSegmentRequests(IReadOnlyCollection<SegmentRequest> segmentRequests)
        {
            var result = new List<SegmentRequest>();
            var servers = _config.GetServers();
            foreach (var segmentRequest in segmentRequests)
            {
                bool isGrpcServerRequest = false;
                foreach (var server in servers)
                {
                    if (segmentRequest.Segment.Spans.Any(s => s.Tags.Any(t => t.Key == Tags.URL && t.Value.StartsWith(server))))
                    {
                        isGrpcServerRequest = true;
                        break;
                    }
                }

                if (!isGrpcServerRequest)
                {
                    result.Add(segmentRequest);
                }
            }

            return result;
        }
    }
}