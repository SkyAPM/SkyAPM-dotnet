using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SmartSql;
using SmartSql.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SkyApm.Diagnostics.SmartSql
{
    public abstract class BaseSmartSqlTracingDiagnosticProcessor
    {
        protected void BeforeDbSessionBeginTransactionSetupSpan(SegmentSpan span, DbSessionBeginTransactionBeforeEventData eventData)
        {
            SetupNewSpan(span);
        }

        protected void BeforeDbSessionCommitSetupSpan(SegmentSpan span, DbSessionCommitBeforeEventData eventData)
        {
            SetupNewSpan(span);
            AddConnectionTag(span, eventData.DbSession.Connection);
        }

        protected void BeforeDbSessionRollbackSetupSpan(SegmentSpan span, DbSessionRollbackBeforeEventData eventData)
        {
            SetupNewSpan(span);
            AddConnectionTag(span, eventData.DbSession.Connection);
        }

        protected void BeforeDbSessionDisposeSetupSpan(SegmentSpan span, DbSessionDisposeBeforeEventData eventData)
        {
            SetupNewSpan(span);
            AddConnectionTag(span, eventData.DbSession.Connection);
        }

        protected void BeforeDbSessionOpenSetupSpan(SegmentSpan span, DbSessionOpenBeforeEventData eventData)
        {
            SetupNewSpan(span);
        }

        protected void AfterDbSessionOpenSetupSpan(SegmentSpan span, DbSessionOpenAfterEventData eventData)
        {
            AddConnectionTag(span, eventData.DbSession.Connection);
        }

        protected void ErrorDbSessionOpenSetupSpan(TracingConfig tracingConfig, SegmentSpan span, DbSessionOpenErrorEventData eventData)
        {
            AddConnectionTag(span, eventData.DbSession.Connection);
            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void BeforeDbSessionInvokeSetupSpan(SegmentSpan span, DbSessionInvokeBeforeEventData eventData)
        {
            SetupNewSpan(span);
        }

        protected void AfterDbSessionInvokeSetupSpan(SegmentSpan span, DbSessionInvokeAfterEventData eventData)
        {
            span.AddTag("from_cache", eventData.ExecutionContext.Result.FromCache);
            var resultSize = eventData.ExecutionContext.Result.IsList
                ? (eventData.ExecutionContext.Result.GetData() as ICollection)?.Count
                : 1;
            span.AddTag("result_size", resultSize?.ToString());
        }

        protected void BeforeCommandExecuterExecuteSetupSpan(SegmentSpan span, CommandExecuterExecuteBeforeEventData eventData)
        {
            SetupNewSpan(span);
            if (eventData.ExecutionContext.Request.RealSql != null)
            {
                span.AddTag(Common.Tags.DB_STATEMENT, eventData.ExecutionContext.Request.RealSql);
            }
        }

        protected void AfterCommandExecuterExecuteSetupSpan(SegmentSpan span, CommandExecuterExecuteAfterEventData eventData)
        {
            AddConnectionTag(span, eventData.ExecutionContext.DbSession.Connection);
        }

        protected void ErrorCommandExecuterExecuteSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CommandExecuterExecuteErrorEventData eventData)
        {
            AddConnectionTag(span, eventData.ExecutionContext.DbSession.Connection);
            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void SetupNewSpan(SegmentSpan span)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = Common.Components.SMART_SQL;
            span.AddTag(Common.Tags.DB_TYPE, "Sql");
        }

        protected string ResolveOperationName(ExecutionContext executionContext)
        {
            return executionContext.Request.FullSqlId != "." ?
                executionContext.Request.FullSqlId : executionContext.Request.ExecutionType.ToString();
        }

        protected void AddConnectionTag(SegmentSpan span, DbConnection dbConnection)
        {
            if (dbConnection == null)
            {
                return;
            }
            if (dbConnection.DataSource != null)
            {
                span.Peer = new Common.StringOrIntValue(dbConnection.DataSource);
            }
            if (dbConnection.Database != null)
            {
                span.AddTag(Common.Tags.DB_INSTANCE, dbConnection.Database);
            }
        }
    }
}
