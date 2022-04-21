using MongoDB.Driver.Core.Events;
using SkyApm.Config;

namespace SkyApm.Diagnostics.MongoDB
{
    public class MongoDiagnosticsProcessorAdapter : IMongoDiagnosticsProcessor
    {
        private readonly IMongoDiagnosticsProcessor _processor;

        public MongoDiagnosticsProcessorAdapter(
            MongoDiagnosticsProcessor defaultProcessor,
            SpanMongoDiagnosticsProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IMongoDiagnosticsProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => "MongoSourceListener";

        [DiagnosticName("MongoActivity.Start")]
        public void BeforeExecuteCommand([Object] CommandStartedEvent @event)
        {
            _processor.BeforeExecuteCommand(@event);
        }

        [DiagnosticName("MongoActivity.Stop")]
        public void AfterExecuteCommand([Object] CommandSucceededEvent @event)
        {
            _processor.AfterExecuteCommand(@event);
        }

        [DiagnosticName("MongoActivity.Failed")]
        public void FailedExecuteCommand([Object] CommandFailedEvent @event)
        {
            _processor.FailedExecuteCommand(@event);
        }
    }
}
