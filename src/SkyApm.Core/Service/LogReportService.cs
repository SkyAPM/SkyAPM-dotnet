using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Service
{
    public class LogReportService : ExecutionService
    {

        private readonly ISkyApmLogDispatcher _dispatcher;
        private readonly TransportConfig _config;
        public LogReportService(IConfigAccessor configAccessor, ISkyApmLogDispatcher dispatcher,
            IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory)
            : base(runtimeEnvironment, loggerFactory)
        {
            _dispatcher = dispatcher;
            _config = configAccessor.Get<TransportConfig>();
            Period = TimeSpan.FromMilliseconds(_config.Interval);
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(3);

        protected override TimeSpan Period { get; }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _dispatcher.Flush(cancellationToken);
        }
        protected override Task Stopping(CancellationToken cancellationToke)
        {
            _dispatcher.Close();
            return Task.CompletedTask;
        }
    }
}
