using System.Threading;
using System.Threading.Tasks;

namespace SkyWalking
{
    public interface IInstrumentationServiceStartup
    {
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}