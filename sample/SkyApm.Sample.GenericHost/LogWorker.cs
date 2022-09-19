using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SkyApm.Sample.GenericHost
{
    public class LogWorker: BackgroundService
    {
        
        private readonly ILogger<LogWorker> _logger;

        public LogWorker(ILogger<LogWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                _logger.LogInformation("LogWorker running at: {}", DateTime.Now);
            }
        }
    }
}
