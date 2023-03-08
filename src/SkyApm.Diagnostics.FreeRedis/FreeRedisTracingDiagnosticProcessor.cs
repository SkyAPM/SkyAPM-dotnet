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

        private SegmentContext CreateFreeRedisLocalSegmentContext(string operation)
        {
            var context = _tracingContext.CreateLocalSegmentContext(operation);
            context.Span.SpanLayer = SpanLayer.CACHE;
            context.Span.Component = Components.Free_Redis;
            context.Span.AddTag(Tags.CACHE_TYPE, "FreeRedis");
            return context;
        }

        #region Notice
        [DiagnosticName(FreeRedis_Notice)]
        public void Notice([Object] NoticeEventArgs eventData)
        {
            var context = CreateFreeRedisLocalSegmentContext(eventData.NoticeType.ToString());
            context.Span.Peer = AnalysisDomain(eventData.Log);
            string cmd = AnalysisCmd(eventData.Log);
            context.Span.AddTag(Tags.CACHE_OP, parseOperation(cmd));
            context.Span.AddTag(Tags.CACHE_CMD, eventData.Log);
            if (eventData?.Exception != null)
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);
        }
        #endregion
        /// <summary>
        /// 解析读写
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 解析命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string AnalysisCmd(string cmd)
        {
            if (cmd == null) return string.Empty;
            cmd.Replace("\r\n", " ");
            char[] cmds = cmd.ToArray();
            StringBuilder stringBuilder = new StringBuilder();
            bool beginAn = false;
            for (var i = 0; i < cmds.Length; i++)
            {
                if (cmds[i] == '>')
                {
                    beginAn = true;
                }
                else if (beginAn && ((cmds[i] >= 'a' && cmds[i] <= 'z') || (cmds[i] >= 'A' && cmds[i] <= 'Z')))
                {
                    stringBuilder.Append(cmds[i]);
                }
                else if (stringBuilder.Length > 0)
                {
                    return stringBuilder.ToString();
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string AnalysisDomain(string cmd)
        {
            if (cmd == null || !cmd.Contains(">")) return string.Empty;
            return cmd.Substring(0, cmd.IndexOf(">")).Trim();
        }
    }
}