using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Data.Common;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public abstract class BaseEntityFrameworkCoreTracingDiagnosticProcessor
    {
        protected void CommandExecutingSetupSpan(SegmentSpan span, CommandEventData eventData, bool logParameterValue)
        {
            span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            span.AddTag(Common.Tags.DB_TYPE, "Sql");
            span.AddTag(Common.Tags.DB_INSTANCE, eventData.Command.Connection.Database);
            span.AddTag(Common.Tags.DB_STATEMENT, eventData.Command.CommandText);
            span.AddTag(Common.Tags.DB_BIND_VARIABLES, BuildParameterVariables(eventData.Command.Parameters, logParameterValue));
        }

        private string BuildParameterVariables(DbParameterCollection dbParameters, bool logParameterValue)
        {
            if (dbParameters == null)
            {
                return string.Empty;
            }

            return dbParameters.FormatParameters(logParameterValue);
        }

        protected void CommandErrorSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CommandErrorEventData eventData)
        {
            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }
    }
}
