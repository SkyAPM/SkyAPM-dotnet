/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Data.Common;
using System.Linq;
using SkyApm.Tracing;
using SkyApm.Config;

namespace SkyApm.Diagnostics.SqlClient
{
    public class SqlClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SqlClientTracingDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }


        public string ListenerName { get; } = SqlClientDiagnosticStrings.DiagnosticListenerName;

        private static string ResolveOperationName(DbCommand sqlCommand)
        {
            var commandType = sqlCommand.CommandText?.Split(' ');
            return $"{SqlClientDiagnosticStrings.SqlClientPrefix}{commandType?.FirstOrDefault()}";
        }

        #region System.Data.SqlClient
        [DiagnosticName(SqlClientDiagnosticStrings.SqlBeforeExecuteCommand)]
        public void BeforeExecuteCommand([Property(Name = "Command")] DbCommand sqlCommand)
        {
            var spanOrSegment = _tracingContext.CreateExit(ResolveOperationName(sqlCommand), sqlCommand.Connection.DataSource);
            spanOrSegment.Span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            spanOrSegment.Span.Component = Common.Components.SQLCLIENT;
            spanOrSegment.Span.AddTag(Common.Tags.DB_TYPE, "sql");
            spanOrSegment.Span.AddTag(Common.Tags.DB_INSTANCE, sqlCommand.Connection.Database);
            spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, sqlCommand.CommandText);
        }


        [DiagnosticName(SqlClientDiagnosticStrings.SqlAfterExecuteCommand)]
        public void AfterExecuteCommand()
        {
            var spanOrSegment = _tracingContext.CurrentExit;
            if (spanOrSegment != null)
            {
                _tracingContext.Finish(spanOrSegment);
            }
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlErrorExecuteCommand)]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            var spanOrSegment = _tracingContext.CurrentExit;
            if (spanOrSegment != null)
            {
                spanOrSegment.Span.ErrorOccurred(ex, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
            }
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