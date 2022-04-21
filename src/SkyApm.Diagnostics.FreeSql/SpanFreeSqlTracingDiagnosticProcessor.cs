using FreeSql.Aop;
using SkyApm.Config;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.FreeSql
{
    public class SpanFreeSqlTracingDiagnosticProcessor : BaseFreeSqlTracingDiagnosticProcessor, IFreeSqlTracingDiagnosticProcessor
    {
        public string ListenerName => "FreeSqlDiagnosticListener";

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;
        public SpanFreeSqlTracingDiagnosticProcessor(
            ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        #region Curd
        [DiagnosticName(FreeSql_CurdBefore)]
        public void CurdBefore([Object] CurdBeforeEventArgs eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.CurdType.ToString());
            CurdBeforeSetupSpan(span, eventData);
        }

        [DiagnosticName(FreeSql_CurdAfter)]
        public void CurdAfter([Object] CurdAfterEventArgs eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CurdAfterSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region SyncStructure
        [DiagnosticName(FreeSql_SyncStructureBefore)]
        public void SyncStructureBefore([Object] SyncStructureBeforeEventArgs eventData)
        {
            var span = _tracingContext.CreateLocalSpan("SyncStructure");
            SyncStructureBeforeSetupSpan(span, eventData);
        }

        [DiagnosticName(FreeSql_SyncStructureAfter)]
        public void SyncStructureAfter([Object] SyncStructureAfterEventArgs eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            SyncStructureAfterSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Command
        [DiagnosticName(FreeSql_CommandBefore)]
        public void CommandBefore([Object] CommandBeforeEventArgs eventData)
        {
            var span = _tracingContext.CreateLocalSpan("Command");
            CommandBeforeSetupSpan(span, eventData);
        }

        [DiagnosticName(FreeSql_CommandAfter)]
        public void CommandAfter([Object] CommandAfterEventArgs eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CommandAfterSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Trace
        [DiagnosticName(FreeSql_TraceBefore)]
        public void TraceBeforeUnitOfWork([Object] TraceBeforeEventArgs eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            TraceBeforeUnitOfWorkSetupSpan(span, eventData);
        }

        [DiagnosticName(FreeSql_TraceAfter)]
        public void TraceAfterUnitOfWork([Object] TraceAfterEventArgs eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            TraceAfterUnitOfWorkSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion
    }
}
