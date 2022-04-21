using MongoDB.Driver.Core.Events;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.MongoDB
{
    public class SpanMongoDiagnosticsProcessor : BaseMongoDiagnosticsProcessor, IMongoDiagnosticsProcessor
    {
        public string ListenerName => "MongoSourceListener";
        private readonly ITracingContext _tracingContext;

        public SpanMongoDiagnosticsProcessor(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        [DiagnosticName("MongoActivity.Start")]
        public void BeforeExecuteCommand([Object] CommandStartedEvent @event)
        {
            var operationName = DiagnosticsActivityEventSubscriber.GetCollectionName(@event);
            var span = _tracingContext.CreateExitSpan(operationName, @event.ConnectionId.ServerId.EndPoint.ToString());

            BeforeExecuteCommandSetupSpan(span, operationName, @event);
        }

        [DiagnosticName("MongoActivity.Stop")]
        public void AfterExecuteCommand([Object] CommandSucceededEvent @event)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterExecuteCommandSetupSpan(span, @event);

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName("MongoActivity.Failed")]
        public void FailedExecuteCommand([Object] CommandFailedEvent @event)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            FailedExecuteCommandSetupSpan(span, @event);

            _tracingContext.StopSpan(span);
        }
    }
}
