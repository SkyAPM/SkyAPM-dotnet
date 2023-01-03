using MassTransit;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.MassTransit
{
    public class MasstransitExecutionService : IExecutionService
    {
        private readonly IBus _busFactory;
        private ConnectHandle busConnectHandle;

        public MasstransitExecutionService(IBus busFactory)
        {
            this._busFactory = busFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            //busConnectHandle = _busFactory.ConnectPublishObserver(publishObserver);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            busConnectHandle.Disconnect();
            return Task.CompletedTask;
        }
    }
}
