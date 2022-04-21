using SkyApm.Tracing.Segments;
using System.Data.Common;
using System.Linq;

namespace SkyApm.Diagnostics.SqlClient
{
    public abstract class BaseSqlClientTracingDiagnosticProcessor
    {
        protected void BeforeExecuteCommandSetupSpan(SegmentSpan span, DbCommand sqlCommand)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = Common.Components.SQLCLIENT;
            span.AddTag(Common.Tags.DB_TYPE, "sql");
            span.AddTag(Common.Tags.DB_INSTANCE, sqlCommand.Connection.Database);
            span.AddTag(Common.Tags.DB_STATEMENT, sqlCommand.CommandText);
        }

        protected string ResolveOperationName(DbCommand sqlCommand)
        {
            var commandType = sqlCommand.CommandText?.Split(' ');
            return $"{SqlClientDiagnosticStrings.SqlClientPrefix}{commandType?.FirstOrDefault()}";
        }
    }
}
