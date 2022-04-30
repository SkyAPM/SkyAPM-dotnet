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
        private readonly TracingConfig _tracingConfig;
        public FreeSqlTracingDiagnosticProcessor(ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        private SpanOrSegmentContext CreateFreeSqlLocalSegmentContext(string operation)
        {
            var spanOrSegment = _tracingContext.CreateLocal(operation);
            spanOrSegment.Span.SpanLayer = SpanLayer.DB;
            spanOrSegment.Span.Component = Common.Components.Free_SQL; 
            spanOrSegment.Span.AddTag(Common.Tags.DB_TYPE, "Sql");
            return spanOrSegment;
        }

        #region Curd
        [DiagnosticName(FreeSql_CurdBefore)]
        public void CurdBefore([Object] CurdBeforeEventArgs eventData)
        {
            var spanOrSegment = CreateFreeSqlLocalSegmentContext(eventData.CurdType.ToString());
            spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
        }
        [DiagnosticName(FreeSql_CurdAfter)]
        public void CurdAfter([Object] CurdAfterEventArgs eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment != null)
            {
                if (eventData?.Exception != null)
                    spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
            }
        }
        #endregion

        #region SyncStructure
        [DiagnosticName(FreeSql_SyncStructureBefore)]
        public void SyncStructureBefore([Object] SyncStructureBeforeEventArgs eventData)
        {
            var spanOrSegment = CreateFreeSqlLocalSegmentContext("SyncStructure");
            spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, string.Join(", ", eventData.EntityTypes.Select(a => a.Name)));
        }
        [DiagnosticName(FreeSql_SyncStructureAfter)]
        public void SyncStructureAfter([Object] SyncStructureAfterEventArgs eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment != null)
            {
                if (string.IsNullOrEmpty(eventData.Sql) == false)
                    spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Sql);
                if (eventData?.Exception != null)
                    spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
            }
        }
        #endregion

        #region Command
        [DiagnosticName(FreeSql_CommandBefore)]
        public void CommandBefore([Object] CommandBeforeEventArgs eventData)
        {
            var spanOrSegment = CreateFreeSqlLocalSegmentContext("Command");
            spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Command.CommandText);
        }
        [DiagnosticName(FreeSql_CommandAfter)]
        public void CommandAfter([Object] CommandAfterEventArgs eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment != null)
            {
                if (string.IsNullOrEmpty(eventData.Log) == false)
                    spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Log);
                if (eventData?.Exception != null)
                    spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
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
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment != null)
            {
                if (string.IsNullOrEmpty(eventData.Remark) == false)
                    spanOrSegment.Span.AddTag(Common.Tags.DB_STATEMENT, eventData.Remark);
                if (eventData?.Exception != null)
                    spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Finish(spanOrSegment);
            }
        }
        #endregion
    }
}
