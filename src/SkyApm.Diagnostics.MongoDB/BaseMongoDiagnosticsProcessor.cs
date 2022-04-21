using MongoDB.Driver.Core.Events;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.MongoDB
{
    public abstract class BaseMongoDiagnosticsProcessor
    {
        protected void BeforeExecuteCommandSetupSpan(SegmentSpan span, string operationName, CommandStartedEvent @event)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = Common.Components.MongoDBCLIENT;
            span.AddTag("db.system", "mongodb");
            span.AddTag("db.name", @event.DatabaseNamespace?.DatabaseName);
            span.AddTag("db.mongodb.collection", operationName);
            span.AddTag("db.operation", operationName + @event.CommandName);
            span.AddTag(Common.Tags.DB_TYPE, "sql");
            span.AddTag(Common.Tags.DB_INSTANCE, @event.DatabaseNamespace.DatabaseName);
            span.AddTag(Common.Tags.DB_STATEMENT, @event.Command.ToString());
        }

        protected void AfterExecuteCommandSetupSpan(SegmentSpan span, CommandSucceededEvent @event)
        {
            span.AddTag(Common.Tags.STATUS_CODE, "ok");
        }

        protected void FailedExecuteCommandSetupSpan(SegmentSpan span, CommandFailedEvent @event)
        {
            span.AddTag("status_description", @event.Failure.Message);
            span.AddTag("error.type", @event.Failure.GetType().FullName);
            span.AddTag("error.msg", @event.Failure.Message);
            span.AddTag("error.stack", @event.Failure.StackTrace);
            span.AddTag(Common.Tags.STATUS_CODE, "error");
        }
    }
}
