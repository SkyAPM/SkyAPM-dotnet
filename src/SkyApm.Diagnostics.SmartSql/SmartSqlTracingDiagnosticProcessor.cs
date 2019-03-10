using System;
using System.Collections.Generic;
using System.Text;
using SkyApm.Tracing;
using SmartSql;
using SmartSql.Diagnostics;

namespace SkyApm.Diagnostics.SmartSql
{
    public class SmartSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => SmartSqlDiagnosticListenerExtensions.SMART_SQL_DIAGNOSTIC_LISTENER;

        private readonly ITracingContext _tracingContext;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;

        public SmartSqlTracingDiagnosticProcessor(ITracingContext tracingContext,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor)
        {
            _tracingContext = tracingContext;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
        }

        private static string ResolveOperationName(ExecutionContext executionContext)
        {
            return executionContext.Request.FullSqlId != "." ?
                executionContext.Request.FullSqlId : executionContext.Request.ExecutionType.ToString();
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_INVOKE)]
        public void BeforeDbSessionInvoke([Object]DbSessionInvokeBeforeEventData eventData)
        {
            var context = _tracingContext.CreateExitSegmentContext(ResolveOperationName(eventData.ExecutionContext), "unkown");
            context.Span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            context.Span.Component = Common.Components.SMART_SQL;
            context.Span.AddTag(Common.Tags.DB_TYPE, "Sql");
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_INVOKE)]
        public void AfterDbSessionInvoke([Object]DbSessionInvokeAfterEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.AddTag("from_cache", eventData.ExecutionContext.Result.FromCache);
                context.Span.Peer = new Common.StringOrIntValue(eventData.ExecutionContext.DbSession.Connection?.DataSource);
                context.Span.AddTag(Common.Tags.DB_INSTANCE, eventData.ExecutionContext.DbSession.Connection?.Database);
                context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.ExecutionContext.Request.RealSql);
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_INVOKE)]
        public void ErrorDbSessionInvoke([Object]DbSessionInvokeErrorEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception);
                _tracingContext.Release(context);
            }
        }
    }
}
