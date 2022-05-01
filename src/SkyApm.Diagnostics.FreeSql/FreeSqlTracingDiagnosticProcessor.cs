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

using FreeSql;
using FreeSql.Aop;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Linq;

namespace SkyApm.Diagnostics.FreeSql
{


    /// <summary>
    /// FreeSqlTracingDiagnosticProcessor
    /// </summary>
    public class FreeSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {

        #region Const

        public const string ComponentName = "FreeSql";

        public const string FreeSql_CurdBefore = "FreeSql.CurdBefore";
        public const string FreeSql_CurdAfter = "FreeSql.CurdAfter";
        public const string FreeSql_SyncStructureBefore = "FreeSql.SyncStructureBefore";
        public const string FreeSql_SyncStructureAfter = "FreeSql.SyncStructureAfter";
        public const string FreeSql_CommandBefore = "FreeSql.CommandBefore";
        public const string FreeSql_CommandAfter = "FreeSql.CommandAfter";
        public const string FreeSql_TraceBefore = "FreeSql.TraceBefore";
        public const string FreeSql_TraceAfter = "FreeSql.TraceAfter";

        #endregion


        public string ListenerName => "FreeSqlDiagnosticListener";

        private readonly ITracingContext _tracingContext;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        public FreeSqlTracingDiagnosticProcessor(ITracingContext tracingContext,
            ILocalSegmentContextAccessor localSegmentContextAccessor, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        private SegmentContext CreateFreeSqlLocalSegmentContext(string operation)
        {
            var context = _tracingContext.CreateLocalSegmentContext(operation);
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = Common.Components.Free_SQL; 
            context.Span.AddTag(Common.Tags.DB_TYPE, "Sql");
            return context;
        }

        #region Curd
        [DiagnosticName(FreeSql_CurdBefore)]
        public void CurdBefore([Object] CurdBeforeEventArgs eventData)
        {
            var context = CreateFreeSqlLocalSegmentContext(eventData.CurdType.ToString());
            context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
        }
        [DiagnosticName(FreeSql_CurdAfter)]
        public void CurdAfter([Object] CurdAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                if (eventData?.Exception != null)
                    context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region SyncStructure
        [DiagnosticName(FreeSql_SyncStructureBefore)]
        public void SyncStructureBefore([Object] SyncStructureBeforeEventArgs eventData)
        {
            var context = CreateFreeSqlLocalSegmentContext("SyncStructure");
            context.Span.AddTag(Common.Tags.DB_STATEMENT, string.Join(", ", eventData.EntityTypes.Select(a => a.Name)));
        }
        [DiagnosticName(FreeSql_SyncStructureAfter)]
        public void SyncStructureAfter([Object] SyncStructureAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                if (!string.IsNullOrEmpty(eventData.Sql))
                    context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
                if (eventData?.Exception != null)
                    context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region Command
        [DiagnosticName(FreeSql_CommandBefore)]
        public void CommandBefore([Object] CommandBeforeEventArgs eventData)
        {
            var context = CreateFreeSqlLocalSegmentContext("Command");
            context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Command.CommandText);
        }
        [DiagnosticName(FreeSql_CommandAfter)]
        public void CommandAfter([Object] CommandAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                if (!string.IsNullOrEmpty(eventData.Log))
                    context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Log);
                if (eventData?.Exception != null)
                    context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region Trace
        [DiagnosticName(FreeSql_TraceBefore)]
        public void TraceBeforeUnitOfWork([Object] TraceBeforeEventArgs eventData)
        {
            var context = CreateFreeSqlLocalSegmentContext(eventData.Operation);

        }
        [DiagnosticName(FreeSql_TraceAfter)]
        public void TraceAfterUnitOfWork([Object] TraceAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                if (!string.IsNullOrEmpty(eventData.Remark))
                    context.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Remark);
                if (eventData?.Exception != null)
                    context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
        #endregion
    }
}
