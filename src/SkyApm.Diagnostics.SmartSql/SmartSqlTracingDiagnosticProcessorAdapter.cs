using SkyApm.Config;
using SmartSql.Diagnostics;

namespace SkyApm.Diagnostics.SmartSql
{
    public class SmartSqlTracingDiagnosticProcessorAdapter : ISmartSqlTracingDiagnosticProcessor
    {
        private readonly ISmartSqlTracingDiagnosticProcessor _processor;

        public SmartSqlTracingDiagnosticProcessorAdapter(
            SmartSqlTracingDiagnosticProcessor defaultProcessor,
            SpanSmartSqlTracingDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (ISmartSqlTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => SmartSqlDiagnosticListenerExtensions.SMART_SQL_DIAGNOSTIC_LISTENER;

        #region BeginTransaction
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_BEGINTRANSACTION)]
        public void BeforeDbSessionBeginTransaction([Object] DbSessionBeginTransactionBeforeEventData eventData)
        {
            _processor.BeforeDbSessionBeginTransaction(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_BEGINTRANSACTION)]
        public void AfterDbSessionBeginTransaction([Object] DbSessionBeginTransactionAfterEventData eventData)
        {
            _processor.AfterDbSessionBeginTransaction(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_BEGINTRANSACTION)]
        public void ErrorDbSessionBeginTransaction([Object] DbSessionBeginTransactionErrorEventData eventData)
        {
            _processor.ErrorDbSessionBeginTransaction(eventData);
        }
        #endregion

        #region Commit
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_COMMIT)]
        public void BeforeDbSessionCommit([Object] DbSessionCommitBeforeEventData eventData)
        {
            _processor.BeforeDbSessionCommit(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_COMMIT)]
        public void AfterDbSessionCommit([Object] DbSessionCommitAfterEventData eventData)
        {
            _processor.AfterDbSessionCommit(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_COMMIT)]
        public void ErrorDbSessionCommit([Object] DbSessionCommitErrorEventData eventData)
        {
            _processor.ErrorDbSessionCommit(eventData);
        }
        #endregion

        #region Rollback
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_ROLLBACK)]
        public void BeforeDbSessionRollback([Object] DbSessionRollbackBeforeEventData eventData)
        {
            _processor.BeforeDbSessionRollback(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_ROLLBACK)]
        public void AfterDbSessionRollback([Object] DbSessionRollbackAfterEventData eventData)
        {
            _processor.AfterDbSessionRollback(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_ROLLBACK)]
        public void ErrorDbSessionRollback([Object] DbSessionRollbackErrorEventData eventData)
        {
            _processor.ErrorDbSessionRollback(eventData);
        }
        #endregion

        #region Dispose
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_DISPOSE)]
        public void BeforeDbSessionDispose([Object] DbSessionDisposeBeforeEventData eventData)
        {
            _processor.BeforeDbSessionDispose(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_DISPOSE)]
        public void AfterDbSessionDispose([Object] DbSessionDisposeAfterEventData eventData)
        {
            _processor.AfterDbSessionDispose(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_DISPOSE)]
        public void ErrorDbSessionDispose([Object] DbSessionDisposeErrorEventData eventData)
        {
            _processor.ErrorDbSessionDispose(eventData);
        }
        #endregion

        #region Open
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_OPEN)]
        public void BeforeDbSessionOpen([Object] DbSessionOpenBeforeEventData eventData)
        {
            _processor.BeforeDbSessionOpen(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_OPEN)]
        public void AfterDbSessionOpen([Object] DbSessionOpenAfterEventData eventData)
        {
            _processor.AfterDbSessionOpen(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_OPEN)]
        public void ErrorDbSessionOpen([Object] DbSessionOpenErrorEventData eventData)
        {
            _processor.ErrorDbSessionOpen(eventData);
        }
        #endregion

        #region Invoke
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_INVOKE)]
        public void BeforeDbSessionInvoke([Object] DbSessionInvokeBeforeEventData eventData)
        {
            _processor.BeforeDbSessionInvoke(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_INVOKE)]
        public void AfterDbSessionInvoke([Object] DbSessionInvokeAfterEventData eventData)
        {
            _processor.AfterDbSessionInvoke(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_INVOKE)]
        public void ErrorDbSessionInvoke([Object] DbSessionInvokeErrorEventData eventData)
        {
            _processor.ErrorDbSessionInvoke(eventData);
        }
        #endregion

        #region CommandExecuter
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_COMMAND_EXECUTER_EXECUTE)]
        public void BeforeCommandExecuterExecute([Object] CommandExecuterExecuteBeforeEventData eventData)
        {
            _processor.BeforeCommandExecuterExecute(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_COMMAND_EXECUTER_EXECUTE)]
        public void AfterCommandExecuterExecute([Object] CommandExecuterExecuteAfterEventData eventData)
        {
            _processor.AfterCommandExecuterExecute(eventData);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_COMMAND_EXECUTER_EXECUTE)]
        public void ErrorCommandExecuterExecute([Object] CommandExecuterExecuteErrorEventData eventData)
        {
            _processor.ErrorCommandExecuterExecute(eventData);
        }
        #endregion
    }
}
