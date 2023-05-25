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
using SkyApm.Transport.Grpc.Common;
using SkyWalking.NetworkProtocol.V3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc
{
    public class LogReporter : ILogReporter
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _grpcConfig;
        private readonly InstrumentConfig _instrumentConfig;

        public LogReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _grpcConfig = configAccessor.Get<GrpcConfig>();
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _logger = loggerFactory.CreateLogger(typeof(LogReporter));
        }
        
        public async Task ReportAsync(IReadOnlyCollection<LogRequest> logRequests,
            CancellationToken cancellationToken = default)
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
                using (var asyncClientStreamingCall = client.collect(_grpcConfig.GetMeta(),
                           _grpcConfig.GetReportTimeout(), cancellationToken))
                {
                    foreach (var logRequest in logRequests)
                    {
                        var logBody = new LogData()
                        {
                            Timestamp = logRequest.Date,
                            Service = _instrumentConfig.ServiceName,
                            ServiceInstance = _instrumentConfig.ServiceInstanceName,
                            Endpoint = logRequest.Endpoint,
                            Body = new LogDataBody()
                            {
                                Type = "text",
                                Text = new TextLog()
                                {
                                    Text = logRequest.Message,
                                },
                            },
                            Tags = new LogTags(),
                        };
                        if (logRequest.SegmentReference != null)
                        {
                            logBody.TraceContext = new TraceContext()
                            {
                                TraceId = logRequest.SegmentReference.TraceId,
                                TraceSegmentId = logRequest.SegmentReference.SegmentId,
                            };
                        }
                        foreach (var tag in logRequest.Tags)
                        {
                            logBody.Tags.Data.Add(new KeyStringValuePair()
                            {
                                Key = tag.Key,
                                Value = tag.Value.ToString(),
                            });
                        }
                        await asyncClientStreamingCall.RequestStream.WriteAsync(logBody);
                    }

                    await asyncClientStreamingCall.RequestStream.CompleteAsync();
                    await asyncClientStreamingCall.ResponseAsync;

                    stopwatch.Stop();
                    _logger.Information($"Report {logRequests.Count} logs. cost: {stopwatch.Elapsed}s");
                }
            }
            catch (IOException ex)
            {
                _logger.Error("Report log fail.", ex);
                _connectionManager.Failure(ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Report log fail.", ex);
            }
        }
    }
}