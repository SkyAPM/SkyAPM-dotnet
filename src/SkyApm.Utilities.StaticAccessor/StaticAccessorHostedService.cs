using Microsoft.Extensions.Hosting;
using SkyApm.Config;
using SkyApm.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class StaticAccessorHostedService : IHostedService
    {
        private readonly ITracingContext _tracingContext;
        private readonly IConfigAccessor _configAccessor;

        public StaticAccessorHostedService(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _configAccessor = configAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SkyApmInstances.TracingContext = _tracingContext;
            SkyApmInstances.TracingConfig = _configAccessor.Get<TracingConfig>();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
