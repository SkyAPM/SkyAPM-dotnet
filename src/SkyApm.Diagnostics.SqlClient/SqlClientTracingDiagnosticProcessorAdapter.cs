using SkyApm.Config;
using System;
using System.Data.Common;

namespace SkyApm.Diagnostics.SqlClient
{
    public class SqlClientTracingDiagnosticProcessorAdapter : ISqlClientTracingDiagnosticProcessor
    {
        private readonly ISqlClientTracingDiagnosticProcessor _processor;

        public SqlClientTracingDiagnosticProcessorAdapter(
            SqlClientTracingDiagnosticProcessor defaultProcessor,
            SpanSqlClientTracingDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (ISqlClientTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName { get; } = SqlClientDiagnosticStrings.DiagnosticListenerName;

        #region System.Data.SqlClient
        [DiagnosticName(SqlClientDiagnosticStrings.SqlBeforeExecuteCommand)]
        public void BeforeExecuteCommand([Property(Name = "Command")] DbCommand sqlCommand)
        {
            _processor.BeforeExecuteCommand(sqlCommand);
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlAfterExecuteCommand)]
        public void AfterExecuteCommand()
        {
            _processor.AfterExecuteCommand();
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlErrorExecuteCommand)]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            _processor.ErrorExecuteCommand(ex);
        }
        #endregion


        #region Microsoft.Data.SqlClient
        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlBeforeExecuteCommand)]
        public void DotNetCoreBeforeExecuteCommand([Property(Name = "Command")] DbCommand sqlCommand)
        {
            _processor.DotNetCoreBeforeExecuteCommand(sqlCommand);
        }

        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlAfterExecuteCommand)]
        public void DotNetCoreAfterExecuteCommand()
        {
            _processor.DotNetCoreAfterExecuteCommand();
        }

        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlErrorExecuteCommand)]
        public void DotNetCoreErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            _processor.DotNetCoreErrorExecuteCommand(ex);
        }
        #endregion
    }
}
