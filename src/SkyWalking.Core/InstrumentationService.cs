using System;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Logging;

namespace SkyWalking.Service
{
    public abstract class InstrumentationService : IInstrumentationService
    {
        private Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;
        
        protected readonly IInstrumentationLogger _logger;
        protected readonly IRuntimeEnvironment _runtimeEnvironment;
        protected readonly IInstrumentationClient _instrumentation;

        protected InstrumentationService(IInstrumentationClient instrumentation, IRuntimeEnvironment runtimeEnvironment, IInstrumentationLoggerFactory loggerFactory)
        {
            _instrumentation = instrumentation;
            _runtimeEnvironment = runtimeEnvironment;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            _timer = new Timer(Callback, source, DueTime, Period);
            _logger.Info($"Start {GetType().Name}.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource?.Cancel();
            _logger.Info($"Stop {GetType().Name}.");
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void Callback(object state)
        {
            if (state is CancellationTokenSource token && !token.IsCancellationRequested && CanExecute())
            {
                await ExecuteAsync(token.Token);
            }
        }
        
        protected virtual bool CanExecute() => _runtimeEnvironment.Initialized;

        protected abstract TimeSpan DueTime { get; }

        protected abstract TimeSpan Period { get; }

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);    
    }
}