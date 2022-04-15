using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport
{
    public interface ILoggerReporter
    {
        Task ReportAsync(IReadOnlyCollection<LoggerRequest> loggerRequests,
           CancellationToken cancellationToken = default(CancellationToken));
    }
}
