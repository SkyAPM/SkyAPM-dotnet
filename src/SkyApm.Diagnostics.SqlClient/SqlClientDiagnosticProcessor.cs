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
        private readonly IExitSegmentContextAccessor _contextAccessor;
        private readonly TracingConfig _tracingConfig;

        public SqlClientTracingDiagnosticProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
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
            var context = _tracingContext.CreateExitSegmentContext(ResolveOperationName(sqlCommand),
                sqlCommand.Connection.DataSource);
            context.Span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            context.Span.Component = Common.Components.SQLCLIENT;
            context.Span.AddTag(Common.Tags.DB_TYPE, "sql");
            context.Span.AddTag(Common.Tags.DB_INSTANCE, sqlCommand.Connection.Database);
            context.Span.AddTag(Common.Tags.DB_STATEMENT, sqlCommand.CommandText);
        }


        [DiagnosticName(SqlClientDiagnosticStrings.SqlAfterExecuteCommand)]
        public void AfterExecuteCommand()
        {
            var context = _contextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }

        [DiagnosticName(SqlClientDiagnosticStrings.SqlErrorExecuteCommand)]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            var context = _contextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(ex, _tracingConfig);
                _tracingContext.Release(context);
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