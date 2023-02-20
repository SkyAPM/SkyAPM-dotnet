using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Config;
using SkyApm.Tracing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class StaticAccessorHostedService : IHostedService
    {
        private readonly ITracingContext _tracingContext;
        private readonly IConfigAccessor _configAccessor;

        public StaticAccessorHostedService(IServiceProvider provider)
        {
            _tracingContext = provider.GetService<ITracingContext>();
            _configAccessor = provider.GetService<IConfigAccessor>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_tracingContext != null && _configAccessor != null)
            {
                SkyApmInstances.TracingContext = _tracingContext;
                SkyApmInstances.ConfigAccessor = _configAccessor;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
