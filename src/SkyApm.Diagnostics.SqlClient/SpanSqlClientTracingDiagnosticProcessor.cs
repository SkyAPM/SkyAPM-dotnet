using System;
using System.Data.Common;
using SkyApm.Tracing;
using SkyApm.Config;

namespace SkyApm.Diagnostics.SqlClient
{
    public class SpanSqlClientTracingDiagnosticProcessor : BaseSqlClientTracingDiagnosticProcessor, ISqlClientTracingDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanSqlClientTracingDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public string ListenerName { get; } = SqlClientDiagnosticStrings.DiagnosticListenerName;

        #region System.Data.SqlClient
        [DiagnosticName(SqlClientDiagnosticStrings.SqlBeforeExecuteCommand)]
        public void BeforeExecuteCommand([Property(Name = "Command")] DbCommand sqlCommand)
        {
            var span = _tracingContext.CreateExitSpan(ResolveOperationName(sqlCommand), sqlCommand.Connection.DataSource);
            BeforeExecuteCommandSetupSpan(span, sqlCommand);
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlAfterExecuteCommand)]
        public void AfterExecuteCommand()
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlErrorExecuteCommand)]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            span.ErrorOccurred(ex, _tracingConfig);
            _tracingContext.StopSpan(span);
        }
        #endregion


        #region Microsoft.Data.SqlClient
        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlBeforeExecuteCommand)]
        public void DotNetCoreBeforeExecuteCommand([Property(Name = "Command")] DbCommand sqlCommand)
        {
            this.BeforeExecuteCommand(sqlCommand);
        }

        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlAfterExecuteCommand)]
        public void DotNetCoreAfterExecuteCommand()
        {
            this.AfterExecuteCommand();
        }

        [DiagnosticName(SqlClientDiagnosticStrings.DotNetCoreSqlErrorExecuteCommand)]
        public void DotNetCoreErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            this.ErrorExecuteCommand(ex);
        }
        #endregion
    }
}
