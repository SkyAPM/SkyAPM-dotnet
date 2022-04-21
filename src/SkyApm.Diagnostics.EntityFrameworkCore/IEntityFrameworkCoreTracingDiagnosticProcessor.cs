using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public interface IEntityFrameworkCoreTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        void CommandExecuting(CommandEventData eventData);

        void CommandExecuted(CommandExecutedEventData eventData);

        void CommandError(CommandErrorEventData eventData);
    }
}
