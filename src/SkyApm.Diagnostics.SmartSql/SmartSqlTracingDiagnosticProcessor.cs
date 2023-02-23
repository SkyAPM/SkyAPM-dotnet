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
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SmartSql;
using SmartSql.Diagnostics;
using SmartSql.Utils;

namespace SkyApm.Diagnostics.SmartSql
{
    public class SmartSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => SmartSqlDiagnosticListenerExtensions.SMART_SQL_DIAGNOSTIC_LISTENER;

        private readonly ITracingContext _tracingContext;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        private readonly IPeerFormatter _peerFormatter;

        public SmartSqlTracingDiagnosticProcessor(ITracingContext tracingContext,
            ILocalSegmentContextAccessor localSegmentContextAccessor, IConfigAccessor configAccessor,
            IPeerFormatter peerFormatter)
        {
            _tracingContext = tracingContext;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _peerFormatter = peerFormatter;
        }
        private void AddConnectionTag(SegmentContext context, DbConnection dbConnection)
        {
            if (dbConnection == null)
            {
                return;
            }
            if (dbConnection.DataSource != null)
            {
                context.Span.Peer = _peerFormatter.GetDbPeer(dbConnection);
            }
            if (dbConnection.Database != null)
            {
                context.Span.AddTag(Common.Tags.DB_INSTANCE, dbConnection.Database);
            }
        }
        private SegmentContext CreateSmartSqlLocalSegmentContext(string operation)
        {
            var context = _tracingContext.CreateLocalSegmentContext(operation);
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = Common.Components.SMART_SQL;
            context.Span.AddTag(Common.Tags.DB_TYPE, "Sql");
            return context;
        }
        #region BeginTransaction
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_BEGINTRANSACTION)]
        public void BeforeDbSessionBeginTransaction([Object]DbSessionBeginTransactionBeforeEventData eventData)
        {
            CreateSmartSqlLocalSegmentContext("BeginTransaction");
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_BEGINTRANSACTION)]
        public void AfterDbSessionBeginTransaction([Object]DbSessionBeginTransactionAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_BEGINTRANSACTION)]
        public void ErrorDbSessionBeginTransaction([Object]DbSessionBeginTransactionErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
        #region Commit
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_COMMIT)]
        public void BeforeDbSessionCommit([Object]DbSessionCommitBeforeEventData eventData)
        {
            var context = CreateSmartSqlLocalSegmentContext(eventData.Operation);
            AddConnectionTag(context, eventData.DbSession.Connection);
        }

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_COMMIT)]
        public void AfterDbSessionCommit([Object]DbSessionCommitAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_COMMIT)]
        public void ErrorDbSessionCommit([Object]DbSessionCommitErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
        #region Rollback
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_ROLLBACK)]
        public void BeforeDbSessionRollback([Object]DbSessionRollbackBeforeEventData eventData)
        {
            var context = CreateSmartSqlLocalSegmentContext(eventData.Operation);
            AddConnectionTag(context, eventData.DbSession.Connection);
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_ROLLBACK)]
        public void AfterDbSessionRollback([Object]DbSessionRollbackAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_ROLLBACK)]
        public void ErrorDbSessionRollback([Object]DbSessionRollbackErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
        #region Dispose
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_DISPOSE)]
        public void BeforeDbSessionDispose([Object]DbSessionDisposeBeforeEventData eventData)
        {
            var context = CreateSmartSqlLocalSegmentContext(eventData.Operation);
            AddConnectionTag(context, eventData.DbSession.Connection);
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_DISPOSE)]
        public void AfterDbSessionDispose([Object]DbSessionDisposeAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_DISPOSE)]
        public void ErrorDbSessionDispose([Object]DbSessionDisposeErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
        #region Open
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_OPEN)]
        public void BeforeDbSessionOpen([Object]DbSessionOpenBeforeEventData eventData)
        {
            CreateSmartSqlLocalSegmentContext(eventData.Operation);
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_OPEN)]
        public void AfterDbSessionOpen([Object]DbSessionOpenAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                AddConnectionTag(context, eventData.DbSession.Connection);
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_OPEN)]
        public void ErrorDbSessionOpen([Object]DbSessionOpenErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                AddConnectionTag(context, eventData.DbSession.Connection);
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
        #region Invoke
        private static string ResolveOperationName(ExecutionContext executionContext)
        {
            return executionContext.Request.FullSqlId != "." ?
                executionContext.Request.FullSqlId : executionContext.Request.ExecutionType.ToString();
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_DB_SESSION_INVOKE)]
        public void BeforeDbSessionInvoke([Object]DbSessionInvokeBeforeEventData eventData)
        {
            CreateSmartSqlLocalSegmentContext(ResolveOperationName(eventData.ExecutionContext));
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_DB_SESSION_INVOKE)]
        public void AfterDbSessionInvoke([Object]DbSessionInvokeAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.AddTag("from_cache", eventData.ExecutionContext.Result.FromCache);
                var resultSize = eventData.ExecutionContext.Result.IsList
                    ? (eventData.ExecutionContext.Result.GetData() as ICollection)?.Count
                    : 1;
                context.Span.AddTag("result_size", resultSize?.ToString());
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_DB_SESSION_INVOKE)]
        public void ErrorDbSessionInvoke([Object]DbSessionInvokeErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region CommandExecuter

        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_BEFORE_COMMAND_EXECUTER_EXECUTE)]
        public void BeforeCommandExecuterExecute([Object]CommandExecuterExecuteBeforeEventData eventData)
        {
            var context = CreateSmartSqlLocalSegmentContext(eventData.Operation);
            if (eventData.ExecutionContext.Request.RealSql != null)
            {
                var sql = eventData.ExecutionContext.FormatSql(true);
                var spliter = "Sql with parameter value: ";
                var startIndex = sql.IndexOf(spliter) + spliter.Length;
                sql = sql.Substring(startIndex);
                context.Span.AddTag(Common.Tags.DB_STATEMENT, sql);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_AFTER_COMMAND_EXECUTER_EXECUTE)]
        public void AfterCommandExecuterExecute([Object]CommandExecuterExecuteAfterEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                AddConnectionTag(context, eventData.ExecutionContext.DbSession.Connection);
                _tracingContext.Release(context);
            }
        }
        [DiagnosticName(SmartSqlDiagnosticListenerExtensions.SMART_SQL_ERROR_COMMAND_EXECUTER_EXECUTE)]
        public void ErrorCommandExecuterExecute([Object]CommandExecuterExecuteErrorEventData eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                AddConnectionTag(context, eventData.ExecutionContext.DbSession.Connection);
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }

        #endregion
    }
}
