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

using FreeRedis;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SkyApm.Diagnostics.FreeRedis
{


    /// <summary>
    /// FreeRedisTracingDiagnosticProcessor
    /// </summary>
    public class FreeRedisTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {

        #region Const

        public const string ComponentName = "FreeRedis";

        public const string FreeRedis_Notice = "FreeRedis.Notice";

        #endregion

        public string ListenerName => "FreeRedisDiagnosticListener";

        private readonly ITracingContext _tracingContext;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        public FreeRedisTracingDiagnosticProcessor(ITracingContext tracingContext,
            ILocalSegmentContextAccessor localSegmentContextAccessor, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }


        #region Notice
        [DiagnosticName(FreeRedis_Notice)]
        public void Notice([Object] NoticeEventArgs eventData)
        {
            var ans = Analysis(eventData.Log);
            if (ans == null) return;
            var context = _tracingContext.CreateExitSegmentContext(ans[2], ans[0]);
            context.Span.SpanLayer = SpanLayer.CACHE;
            context.Span.Component = Components.Free_Redis;
            context.Span.AddTag(Tags.CACHE_TYPE, "FreeRedis");
            context.Span.AddTag(Tags.CACHE_OP, parseOperation(ans[2]));
            context.Span.AddTag(Tags.CACHE_CMD, ans[1]);
            context.Span.AddTag("result", ans[3]);
            context.Span.AddTag("exec_time", ans[4]);
            if (eventData?.Exception != null)
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);
        }
        #endregion
        private static String parseOperation(String cmd)
        {
            if (FreeRedisPluginConfig.OPERATION_MAPPING_READ.Contains(cmd))
            {
                return "read";
            }
            if (FreeRedisPluginConfig.OPERATION_MAPPING_WRITE.Contains(cmd))
            {
                return "write";
            }
            return string.Empty;
        }
        private static Regex anRex = new Regex("(.+)\\s>\\s(([A-Z]+)\\s*.*?)\\r\\n(.*?)\\r\\n\\((\\d+)ms\\)\\r\\n");
        /// <summary>
        /// Analysis
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>[address,cmd,op,result,exectime]</returns>
        public static string[] Analysis(string cmd)
        {
            if (!(cmd == null || !cmd.Contains(">")))
            {
                if (anRex.IsMatch(cmd))
                {
                    var matchs = anRex.Match(cmd);
                    if (matchs.Success && matchs.Groups.Count > 5)
                    {
                        return new string[] {
                            matchs.Groups[1].Value,
                            matchs.Groups[2].Value,
                            matchs.Groups[3].Value,
                            matchs.Groups[4].Value,
                            matchs.Groups[5].Value,
                        };
                    }
                }
            }
            return null;
        }
    }
}