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
using SkyWalking.NetworkProtocol.V3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc
{
    public class LoggerReporter : ILoggerReporter
    {

        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        public LoggerReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _config = configAccessor.Get<GrpcConfig>();
            _logger = loggerFactory.CreateLogger(typeof(SegmentReporter));
        }


        public async Task ReportAsync(IReadOnlyCollection<LoggerRequest> loggerRequests, CancellationToken cancellationToken = default)
        {
            if (!_connectionManager.Ready)
            {
                return;
            }

            var connection = _connectionManager.GetConnection();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var client = new LogReportService.LogReportServiceClient(connection);
                using (var asyncClientStreamingCall = client.collect(_config.GetMeta(), _config.GetReportTimeout(), cancellationToken))
                {
                    foreach (var loggerRequest in loggerRequests)
                    {
                        StringBuilder logmessage=new StringBuilder();
                        foreach (var log in loggerRequest.Logs)
                        {
                            logmessage.Append($"\r\n{log.Key} : {log.Value}");
                        }
                        var logbody = new LogData()
                        {
                            TraceContext = new TraceContext()
                            {
                                TraceId = loggerRequest.SegmentRequest.TraceId,
                                TraceSegmentId = loggerRequest.SegmentRequest.Segment.SegmentId,
                                //SpanId=item.Segment
                            },
                            Timestamp = loggerRequest.Date,
                            Service = loggerRequest.SegmentRequest.Segment.ServiceId,
                            ServiceInstance = loggerRequest.SegmentRequest.Segment.ServiceInstanceId,
                            Endpoint = "",
                            Body = new LogDataBody()
                            {
                                Type = "text",
                                Text = new TextLog()
                                {
                                    Text = logmessage.ToString(),
                                },
                            },
                        };
                        await asyncClientStreamingCall.RequestStream.WriteAsync(logbody);
                    }

                    await asyncClientStreamingCall.RequestStream.CompleteAsync();
                    await asyncClientStreamingCall.ResponseAsync;

                    stopwatch.Stop();
                    _logger.Information($"Report {loggerRequests.Count} logger logger. cost: {stopwatch.Elapsed}s");
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Report trace segment fail.", ex);
                _connectionManager.Failure(ex);

            }

        }
    }
}
