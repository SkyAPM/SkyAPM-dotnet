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
 
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Data.Common;
using System.Linq;

 namespace SkyApm.ClrProfiler.Trace.MySqlData
{
    public class MySqlDataClient : AbsMethodWrapper
    {
        private const string TypeName = "MySql.Data.MySqlClient.MySqlCommand";
        private static readonly string[] AssemblyNames = { "MySql.Data" };
        private static readonly string[] TraceMethods = { "ExecuteReader", "ExecuteNonQuery", "ExecuteScalar" };

        private readonly ITracingContext _tracingContext;

        public MySqlDataClient(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            var dbCommand = (DbCommand)traceMethodInfo.InvocationTarget;

            var operationName = $"DB {traceMethodInfo.MethodBase.Name}";
            var context = _tracingContext.CreateExitSegmentContext(operationName, dbCommand.Connection.DataSource);
            context.Span.Component = Components.MYSQL;
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.AddTag(Common.Tags.DB_TYPE, "Sql");
            context.Span.AddTag(Common.Tags.DB_INSTANCE, dbCommand.Connection.Database);
            context.Span.AddTag(Common.Tags.DB_STATEMENT, dbCommand.CommandText);
            context.Span.AddTag(Common.Tags.DB_BIND_VARIABLES, BuildParameterVariables(dbCommand.Parameters));

            traceMethodInfo.TraceContext = context;

            return delegate (object returnValue, Exception ex)
            {
                Leave(traceMethodInfo,returnValue,ex);
            };
        }

        private string BuildParameterVariables(DbParameterCollection dbParameters)
        {
            if (dbParameters == null)
            {
                return string.Empty;
            }

            return dbParameters.FormatParameters(false);
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {
            var context = (SegmentContext)traceMethodInfo.TraceContext;
            if (ex != null)
            {
                context.Span.ErrorOccurred(ex);
            }

            _tracingContext.Release(context);
        }

        public override bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            var invocationTargetType = traceMethodInfo.Type;
            var assemblyName = invocationTargetType.Assembly.GetName().Name;
            if (AssemblyNames.Contains(assemblyName) && TypeName == invocationTargetType.FullName)
            {
                if (TraceMethods.Contains(traceMethodInfo.MethodBase.Name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

