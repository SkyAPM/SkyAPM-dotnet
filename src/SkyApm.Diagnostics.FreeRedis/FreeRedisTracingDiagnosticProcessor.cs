using FreeRedis;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Linq;

namespace SkyApm.Diagnostics.FreeRedis
{


    /// <summary>
    /// FreeRedisTracingDiagnosticProcessor
    /// </summary>
    public class FreeRedisTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public static readonly StringOrIntValue Free_Redis = new StringOrIntValue(7, "Redis");

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
            context.Span.Component = Free_Redis;
            context.Span.AddTag("cache.type", "FreeRedis");
            return context;
        }

        #region Notice
        [DiagnosticName(FreeRedis_Notice)]
        public void Notice([Object] NoticeEventArgs eventData)
        {
            var context = CreateFreeRedisLocalSegmentContext(eventData.NoticeType.ToString());
            context.Span.AddTag("cache.statement", eventData.Log);
            if (eventData?.Exception != null)
                context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);

        }
        #endregion
    }
}