using System;
using System.Data.Common;

namespace SkyApm.Diagnostics.SqlClient
{
    public interface ISqlClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        #region System.Data.SqlClient
        void BeforeExecuteCommand(DbCommand sqlCommand);

        void AfterExecuteCommand();

        void ErrorExecuteCommand(Exception ex);
        #endregion

        #region Microsoft.Data.SqlClient
        void DotNetCoreBeforeExecuteCommand(DbCommand sqlCommand);

        void DotNetCoreAfterExecuteCommand();

        void DotNetCoreErrorExecuteCommand(Exception ex);
        #endregion
    }
}
