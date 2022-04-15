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
                            logmessage.Append($"\r\n{log.Key}:{log.Value}");
                        }
                        var logbody = new LogData()
                        {
                            TraceContext = new TraceContext()
                            {
                                TraceId = loggerRequest.SegmentRequest.TraceId,
                                TraceSegmentId = loggerRequest.SegmentRequest.Segment.SegmentId,
                                //SpanId=item.Segment
                            },
                            Timestamp = DateTimeOffset.UtcNow.Ticks,
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
