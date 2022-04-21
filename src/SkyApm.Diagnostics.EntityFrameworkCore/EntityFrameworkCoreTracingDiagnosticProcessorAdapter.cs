using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkyApm.Config;
using System;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public class EntityFrameworkCoreTracingDiagnosticProcessorAdapter : ITracingDiagnosticProcessor
    {
        private readonly IEntityFrameworkCoreTracingDiagnosticProcessor _processor;

        public EntityFrameworkCoreTracingDiagnosticProcessorAdapter(
            EntityFrameworkCoreTracingDiagnosticProcessor defaultProcessor,
            SpanEntityFrameworkCoreTracingDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IEntityFrameworkCoreTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => DbLoggerCategory.Name;

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void CommandExecuting([Object] CommandEventData eventData)
        {
            _processor.CommandExecuting(eventData);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void CommandExecuted([Object] CommandExecutedEventData eventData)
        {
            _processor.CommandExecuted(eventData);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void CommandError([Object] CommandErrorEventData eventData)
        {
            _processor.CommandError(eventData);
        }
    }
}
