using System;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Logging;

namespace SkyWalking.Service
{
    public class ServiceDiscoveryService : InstrumentationService
    {
        protected override TimeSpan DueTime { get; } = TimeSpan.Zero;

        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(30);

        public ServiceDiscoveryService(IInstrumentationClient client, IRuntimeEnvironment runtimeEnvironment, IInstrumentationLoggerFactory loggerFactory)
            : base(client, runtimeEnvironment, loggerFactory)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Heartbeat(cancellationToken);
        }

        protected override bool CanExecute() => true;

        private async Task RegisterApplication(CancellationToken cancellationToken)
        {
            if (!_runtimeEnvironment.ApplicationId.HasValue)
            {
                
            }
        }
        
        private async Task Heartbeat(CancellationToken cancellationToken)
        {
            if (_runtimeEnvironment.Initialized)
            {
                try
                {
                    await _instrumentation.HeartbeatAsync(_runtimeEnvironment.ApplicationInstanceId.Value, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.Error("Heartbeat error.", e);
                }
            }
        }
    }
}
