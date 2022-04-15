using SkyApm.Tracing.Segments;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport
{
    public interface ISkyApmLogDispatcher
    {
        bool Dispatch(LoggerContext loggerContext);

        Task Flush(CancellationToken token = default(CancellationToken));

        void Close();

    }
}
