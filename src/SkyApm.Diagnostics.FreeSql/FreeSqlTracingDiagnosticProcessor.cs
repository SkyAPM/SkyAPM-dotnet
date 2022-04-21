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

using FreeSql.Aop;
using SkyApm.Config;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.FreeSql
{


    /// <summary>
    /// FreeSqlTracingDiagnosticProcessor
    /// </summary>
    public class FreeSqlTracingDiagnosticProcessor : BaseFreeSqlTracingDiagnosticProcessor, IFreeSqlTracingDiagnosticProcessor
    {
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

        #region Curd
        [DiagnosticName(FreeSql_CurdBefore)]
        public void CurdBefore([Object] CurdBeforeEventArgs eventData)
        {
            var context = _tracingContext.CreateLocalSegmentContext(eventData.CurdType.ToString());
            CurdBeforeSetupSpan(context.Span, eventData);
        }

        [DiagnosticName(FreeSql_CurdAfter)]
        public void CurdAfter([Object] CurdAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                CurdAfterSetupSpan(_tracingConfig, context.Span, eventData);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region SyncStructure
        [DiagnosticName(FreeSql_SyncStructureBefore)]
        public void SyncStructureBefore([Object] SyncStructureBeforeEventArgs eventData)
        {
            var context = _tracingContext.CreateLocalSegmentContext("SyncStructure");
            SyncStructureBeforeSetupSpan(context.Span, eventData);
        }

        [DiagnosticName(FreeSql_SyncStructureAfter)]
        public void SyncStructureAfter([Object] SyncStructureAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                SyncStructureAfterSetupSpan(_tracingConfig, context.Span, eventData);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region Command
        [DiagnosticName(FreeSql_CommandBefore)]
        public void CommandBefore([Object] CommandBeforeEventArgs eventData)
        {
            var context = _tracingContext.CreateLocalSegmentContext("Command");
            CommandBeforeSetupSpan(context.Span, eventData);
        }

        [DiagnosticName(FreeSql_CommandAfter)]
        public void CommandAfter([Object] CommandAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                CommandAfterSetupSpan(_tracingConfig, context.Span, eventData);
                _tracingContext.Release(context);
            }
        }
        #endregion

        #region Trace
        [DiagnosticName(FreeSql_TraceBefore)]
        public void TraceBeforeUnitOfWork([Object] TraceBeforeEventArgs eventData)
        {
            var context = _tracingContext.CreateLocalSegmentContext(eventData.Operation);
            TraceBeforeUnitOfWorkSetupSpan(context.Span, eventData);
        }

        [DiagnosticName(FreeSql_TraceAfter)]
        public void TraceAfterUnitOfWork([Object] TraceAfterEventArgs eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context != null)
            {
                TraceAfterUnitOfWorkSetupSpan(_tracingConfig, context.Span, eventData);
                _tracingContext.Release(context);
            }
        }
        #endregion
    }
}
