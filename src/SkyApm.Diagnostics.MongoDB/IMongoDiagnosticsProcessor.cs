using MongoDB.Driver.Core.Events;

namespace SkyApm.Diagnostics.MongoDB
{
    public interface IMongoDiagnosticsProcessor : ITracingDiagnosticProcessor
    {
        void BeforeExecuteCommand(CommandStartedEvent @event);

        void AfterExecuteCommand(CommandSucceededEvent @event);

        void FailedExecuteCommand(CommandFailedEvent @event);
    }
}
