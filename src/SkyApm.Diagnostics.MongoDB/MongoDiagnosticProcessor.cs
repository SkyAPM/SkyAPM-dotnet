using MongoDB.Driver.Core.Events;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.MongoDB
{
    public class MongoDiagnosticsProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => "MongoSourceListener";
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _contextAccessor;
 
        public MongoDiagnosticsProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
        }

        [DiagnosticName("MongoActivity.Start")]
        public void BeforeExecuteCommand([Object] CommandStartedEvent @event)
        {
            var operationName = DiagnosticsActivityEventSubscriber.GetCollectionName(@event);
            var context = _tracingContext.CreateExitSegmentContext(operationName, @event.ConnectionId.ServerId.EndPoint.ToString());
            context.Span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            context.Span.Component = Common.Components.MongoDBLIENT;
            context.Span.AddTag("db.system", "mongodb");
            context.Span.AddTag("db.name", @event.DatabaseNamespace?.DatabaseName);
            context.Span.AddTag("db.mongodb.collection", operationName);
            context.Span.AddTag("db.operation", operationName + @event.CommandName);
            context.Span.AddTag(Common.Tags.DB_TYPE, "sql");
            context.Span.AddTag(Common.Tags.DB_INSTANCE, @event.DatabaseNamespace.DatabaseName);
            context.Span.AddTag(Common.Tags.DB_STATEMENT, @event.Command.ToString());
        }

        [DiagnosticName("MongoActivity.Stop")]
        public void AfterExecuteCommand([Object] CommandSucceededEvent @event)
        {
            Console.WriteLine(@event.Duration.TotalMilliseconds);
            var context = _contextAccessor.Context;
            context?.Span.AddTag(Common.Tags.STATUS_CODE, "ok");

            _tracingContext.Release(context);
        }

        [DiagnosticName("MongoActivity.Failed")]
        public void FailedExecuteCommand([Object] CommandFailedEvent @event)
        {
            var context = _contextAccessor.Context;
            context?.Span.AddTag("status_description", @event.Failure.Message);
            context?.Span.AddTag("error.type", @event.Failure.GetType().FullName);
            context?.Span.AddTag("error.msg", @event.Failure.Message);
            context?.Span.AddTag("error.stack", @event.Failure.StackTrace);
            context?.Span.AddTag(Common.Tags.STATUS_CODE, "error");

            _tracingContext.Release(context);
        }
         
    }
}
