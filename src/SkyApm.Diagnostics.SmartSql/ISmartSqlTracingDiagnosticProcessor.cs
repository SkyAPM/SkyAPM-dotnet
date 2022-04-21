using SmartSql.Diagnostics;

namespace SkyApm.Diagnostics.SmartSql
{
    public interface ISmartSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        #region BeginTransaction
        void BeforeDbSessionBeginTransaction(DbSessionBeginTransactionBeforeEventData eventData);

        void AfterDbSessionBeginTransaction(DbSessionBeginTransactionAfterEventData eventData);

        void ErrorDbSessionBeginTransaction(DbSessionBeginTransactionErrorEventData eventData);
        #endregion

        #region Commit
        void BeforeDbSessionCommit(DbSessionCommitBeforeEventData eventData);

        void AfterDbSessionCommit(DbSessionCommitAfterEventData eventData);

        void ErrorDbSessionCommit(DbSessionCommitErrorEventData eventData);
        #endregion

        #region Rollback
        void BeforeDbSessionRollback(DbSessionRollbackBeforeEventData eventData);

        void AfterDbSessionRollback(DbSessionRollbackAfterEventData eventData);

        void ErrorDbSessionRollback(DbSessionRollbackErrorEventData eventData);
        #endregion

        #region Dispose
        void BeforeDbSessionDispose(DbSessionDisposeBeforeEventData eventData);

        void AfterDbSessionDispose(DbSessionDisposeAfterEventData eventData);

        void ErrorDbSessionDispose(DbSessionDisposeErrorEventData eventData);
        #endregion

        #region Open
        void BeforeDbSessionOpen(DbSessionOpenBeforeEventData eventData);

        void AfterDbSessionOpen(DbSessionOpenAfterEventData eventData);

        void ErrorDbSessionOpen(DbSessionOpenErrorEventData eventData);
        #endregion

        #region Invoke
        void BeforeDbSessionInvoke(DbSessionInvokeBeforeEventData eventData);

        void AfterDbSessionInvoke(DbSessionInvokeAfterEventData eventData);

        void ErrorDbSessionInvoke(DbSessionInvokeErrorEventData eventData);
        #endregion

        #region CommandExecuter
        void BeforeCommandExecuterExecute(CommandExecuterExecuteBeforeEventData eventData);

        void AfterCommandExecuterExecute(CommandExecuterExecuteAfterEventData eventData);

        void ErrorCommandExecuterExecute(CommandExecuterExecuteErrorEventData eventData);
        #endregion
    }
}
