using FreeSql.Aop;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Linq;

namespace SkyApm.Diagnostics.FreeSql
{
    public abstract class BaseFreeSqlTracingDiagnosticProcessor
    {
        #region Const

        public const string ComponentName = "FreeSql";

        public const string FreeSql_CurdBefore = "FreeSql.CurdBefore";
        public const string FreeSql_CurdAfter = "FreeSql.CurdAfter";
        public const string FreeSql_SyncStructureBefore = "FreeSql.SyncStructureBefore";
        public const string FreeSql_SyncStructureAfter = "FreeSql.SyncStructureAfter";
        public const string FreeSql_CommandBefore = "FreeSql.CommandBefore";
        public const string FreeSql_CommandAfter = "FreeSql.CommandAfter";
        public const string FreeSql_TraceBefore = "FreeSql.TraceBefore";
        public const string FreeSql_TraceAfter = "FreeSql.TraceAfter";

        #endregion

        protected void CurdBeforeSetupSpan(SegmentSpan span, CurdBeforeEventArgs eventData)
        {
            SetupNewSpan(span);
            span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
        }

        protected void CurdAfterSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CurdAfterEventArgs eventData)
        {
            if (eventData?.Exception != null)
                span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void SyncStructureBeforeSetupSpan(SegmentSpan span, SyncStructureBeforeEventArgs eventData)
        {
            SetupNewSpan(span);
            span.AddTag(Common.Tags.DB_STATEMENT, string.Join(", ", eventData.EntityTypes.Select(a => a.Name)));
        }

        protected void SyncStructureAfterSetupSpan(TracingConfig tracingConfig, SegmentSpan span, SyncStructureAfterEventArgs eventData)
        {
            if (string.IsNullOrEmpty(eventData.Sql) == false)
                span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
            if (eventData?.Exception != null)
                span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void CommandBeforeSetupSpan(SegmentSpan span, CommandBeforeEventArgs eventData)
        {
            SetupNewSpan(span);
            span.AddTag(Common.Tags.DB_STATEMENT, eventData.Command.CommandText);
        }

        protected void CommandAfterSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CommandAfterEventArgs eventData)
        {
            if (string.IsNullOrEmpty(eventData.Log) == false)
                span.AddTag(Common.Tags.DB_STATEMENT, eventData.Log);
            if (eventData?.Exception != null)
                span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void TraceBeforeUnitOfWorkSetupSpan(SegmentSpan span, TraceBeforeEventArgs eventData)
        {
            SetupNewSpan(span);
        }

        protected void TraceAfterUnitOfWorkSetupSpan(TracingConfig tracingConfig, SegmentSpan span, TraceAfterEventArgs eventData)
        {
            if (string.IsNullOrEmpty(eventData.Remark) == false)
                span.AddTag(Common.Tags.DB_STATEMENT, eventData.Remark);
            if (eventData?.Exception != null)
                span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected void SetupNewSpan(SegmentSpan span)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = Common.Components.Free_SQL;
            span.AddTag(Common.Tags.DB_TYPE, "Sql");
        }
    }
}
