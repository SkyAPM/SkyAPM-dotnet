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

using Microsoft.Extensions.DependencyInjection;
using SkyApm.Utilities.DependencyInjection;
using System.Diagnostics;
// ReSharper disable UnusedMethodReturnValue.Global

namespace SkyApm.Diagnostics.FreeSql;

public static class SkyWalkingBuilderExtensions
{
    private static readonly DiagnosticListener dl = new("FreeSqlDiagnosticListener");

    public static SkyApmExtensions AddFreeSql(this SkyApmExtensions extensions, IFreeSql fsql)
    {
        if (extensions == null)
        {
            throw new ArgumentNullException(nameof(extensions));
        }
        _ = extensions.Services.AddSingleton<ITracingDiagnosticProcessor, FreeSqlTracingDiagnosticProcessor>();
        if (fsql != null)
        {
            ConfigAop(fsql);
        }
        return extensions;
    }


    private static void ConfigAop(IFreeSql fsql)
    {
        fsql.Aop.CurdBefore += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_CurdBefore, e);
        fsql.Aop.CurdAfter += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_CurdAfter, e);

        fsql.Aop.SyncStructureBefore += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_SyncStructureBefore, e);
        fsql.Aop.SyncStructureAfter += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_SyncStructureAfter, e);

        fsql.Aop.CommandBefore += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_CommandBefore, e);
        fsql.Aop.CommandAfter += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_CommandAfter, e);

        fsql.Aop.TraceBefore += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_TraceBefore, e);
        fsql.Aop.TraceAfter += (s, e) => dl.Write(FreeSqlTracingDiagnosticProcessor.FreeSql_TraceAfter, e);
    }
}