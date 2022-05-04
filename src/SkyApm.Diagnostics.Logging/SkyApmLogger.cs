using Microsoft.Extensions.Options;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System;
using System.Collections.Generic;

namespace SkyApm.Diagnostics.Logging
{

    public class SkyApmLogger <TCategoryName> : ISkyApmLogger<TCategoryName>
    {

        private readonly bool _pushSkywalking;
        private readonly Type _loggerName;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        public SkyApmLogger(IEntrySegmentContextAccessor entrySegmentContextAccessor, ISkyApmLogDispatcher skyApmLogDispatcher, IOptions<LogPushSkywalkingConfig> logPushSkywalkingConfig)
        {
            _loggerName = typeof(TCategoryName);
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _pushSkywalking = logPushSkywalkingConfig.Value.Enable;
        }

        public void Debug(string message)
        {
            SendLog("Debug", message);
        }

        public void Error(string message, Exception exception)
        {
            SendLog("Error", message);
        }

        public void Information(string message)
        {
            SendLog("Information", message);
        }

        public void Trace(string message)
        {
            SendLog("Trace", message);
        }

        public void Warning(string message)
        {
            SendLog("Warning", message);
        }

        private void SendLog(string logLevel, string message)
        {
            if (_pushSkywalking)
            {
                var logs = new Dictionary<string, object>();
                logs.Add("className", _loggerName);
                logs.Add("Level", logLevel);
                logs.Add("logMessage", message);
                var logContext = new LoggerContext()
                {
                    Logs = logs,
                    SegmentContext = _entrySegmentContextAccessor.Context,
                    Date = DateTimeOffset.UtcNow.Offset.Ticks
                };
                _skyApmLogDispatcher.Dispatch(logContext);
            }

        }

    }

    //public class SkyApmLoggerFactory : ILoggerFactory
    //{
    //    public ILogger CreateLogger(Type type)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}