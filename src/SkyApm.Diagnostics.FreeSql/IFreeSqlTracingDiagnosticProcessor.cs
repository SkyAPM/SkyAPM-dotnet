using FreeSql.Aop;

namespace SkyApm.Diagnostics.FreeSql
{
    public interface IFreeSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        #region Curd
        void CurdBefore(CurdBeforeEventArgs eventData);

        void CurdAfter(CurdAfterEventArgs eventData);
        #endregion

        #region SyncStructure
        void SyncStructureBefore(SyncStructureBeforeEventArgs eventData);

        void SyncStructureAfter(SyncStructureAfterEventArgs eventData);
        #endregion

        #region Command
        void CommandBefore(CommandBeforeEventArgs eventData);

        void CommandAfter(CommandAfterEventArgs eventData);
        #endregion

        #region Trace
        void TraceBeforeUnitOfWork(TraceBeforeEventArgs eventData);

        void TraceAfterUnitOfWork(TraceAfterEventArgs eventData);
        #endregion
    }
}
