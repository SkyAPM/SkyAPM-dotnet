using System;
using SkyApm.Config;
using SkyApm.Tracing;
using SmartSql.Diagnostics;

namespace SkyApm.Diagnostics.SmartSql
{
    public class SpanSmartSqlTracingDiagnosticProcessor : BaseSmartSqlTracingDiagnosticProcessor, ISmartSqlTracingDiagnosticProcessor
    {
        public string ListenerName => SmartSqlDiagnosticListenerExtensions.SMART_SQL_DIAGNOSTIC_LISTENER;

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanSmartSqlTracingDiagnosticProcessor(
            ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        #region BeginTransaction
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_BEGINTRANSACTION)]
        public void BeforeDbSessionBeginTransaction([Object] DbSessionBeginTransactionBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan("BeginTransaction");
            BeforeDbSessionBeginTransactionSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_BEGINTRANSACTION)]
        public void AfterDbSessionBeginTransaction([Object] DbSessionBeginTransactionAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_BEGINTRANSACTION)]
        public void ErrorDbSessionBeginTransaction([Object] DbSessionBeginTransactionErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Commit
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_COMMIT)]
        public void BeforeDbSessionCommit([Object] DbSessionCommitBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            BeforeDbSessionCommitSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_COMMIT)]
        public void AfterDbSessionCommit([Object] DbSessionCommitAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_COMMIT)]
        public void ErrorDbSessionCommit([Object] DbSessionCommitErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Rollback
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_ROLLBACK)]
        public void BeforeDbSessionRollback([Object] DbSessionRollbackBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            BeforeDbSessionRollbackSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_ROLLBACK)]
        public void AfterDbSessionRollback([Object] DbSessionRollbackAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_ROLLBACK)]
        public void ErrorDbSessionRollback([Object] DbSessionRollbackErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Dispose
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_DISPOSE)]
        public void BeforeDbSessionDispose([Object] DbSessionDisposeBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            BeforeDbSessionDisposeSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_DISPOSE)]
        public void AfterDbSessionDispose([Object] DbSessionDisposeAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_DISPOSE)]
        public void ErrorDbSessionDispose([Object] DbSessionDisposeErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Open
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_OPEN)]
        public void BeforeDbSessionOpen([Object] DbSessionOpenBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            BeforeDbSessionOpenSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_OPEN)]
        public void AfterDbSessionOpen([Object] DbSessionOpenAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterDbSessionOpenSetupSpan(span, eventData);
            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_OPEN)]
        public void ErrorDbSessionOpen([Object] DbSessionOpenErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            ErrorDbSessionOpenSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region Invoke
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_INVOKE)]
        public void BeforeDbSessionInvoke([Object] DbSessionInvokeBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(ResolveOperationName(eventData.ExecutionContext));
            BeforeDbSessionInvokeSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_INVOKE)]
        public void AfterDbSessionInvoke([Object] DbSessionInvokeAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterDbSessionInvokeSetupSpan(span, eventData);
            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_INVOKE)]
        public void ErrorDbSessionInvoke([Object] DbSessionInvokeErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion

        #region CommandExecuter
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_COMMAND_EXECUTER_EXECUTE)]
        public void BeforeCommandExecuterExecute([Object] CommandExecuterExecuteBeforeEventData eventData)
        {
            var span = _tracingContext.CreateLocalSpan(eventData.Operation);
            BeforeCommandExecuterExecuteSetupSpan(span, eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_COMMAND_EXECUTER_EXECUTE)]
        public void AfterCommandExecuterExecute([Object] CommandExecuterExecuteAfterEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterCommandExecuterExecuteSetupSpan(span, eventData);
            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_COMMAND_EXECUTER_EXECUTE)]
        public void ErrorCommandExecuterExecute([Object] CommandExecuterExecuteErrorEventData eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            ErrorCommandExecuterExecuteSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
        #endregion
    }
}
