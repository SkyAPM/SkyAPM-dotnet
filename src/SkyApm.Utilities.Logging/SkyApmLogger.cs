using Microsoft.Extensions.DependencyInjection;
using SkyApm.Logging;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System;
using System.Collections.Generic;

namespace SkyApm.Utilities.Logging
{
    public class SkyApmLogger : ILogger
    {
        private readonly Type _loggerName;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        private readonly bool _pushSkywalking;
        public SkyApmLogger(Type type, IServiceProvider serviceProvider,bool pushSkywalking)
        {
            _entrySegmentContextAccessor = serviceProvider.GetService<IEntrySegmentContextAccessor>();
            _skyApmLogDispatcher = serviceProvider.GetService<ISkyApmLogDispatcher>();
            _loggerName = type;
            _pushSkywalking= pushSkywalking;
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
            if(_pushSkywalking)
            {
                var logs = new Dictionary<string, object>();
                logs.Add("className", _loggerName);
                logs.Add("Level", logLevel);
                logs.Add("logMessage", message);
                var logContext = new LoggerContext()
                {
                    Logs = logs,
                    SegmentContext = _entrySegmentContextAccessor.Context,
                };
                _skyApmLogDispatcher.Dispatch(logContext);
            }
            
        }
    }
}
