using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;

namespace SkyApm.Diagnostics.Logging
{
    public class SkyApmLogger:ILogger
    {
        private readonly string _categoryName;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        public SkyApmLogger(string categoryName, ISkyApmLogDispatcher skyApmLogDispatcher, IEntrySegmentContextAccessor entrySegmentContextAccessor)
        {
            _categoryName = categoryName;
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (true)
            {
                var logs = new Dictionary<string, object>();
                logs.Add("className", _categoryName);
                logs.Add("Level", logLevel);
                logs.Add("logMessage", state.ToString()??"");
                var logContext = new LoggerContext()
                {
                    Logs = logs,
                    SegmentContext = _entrySegmentContextAccessor.Context,
                };
                _skyApmLogDispatcher.Dispatch(logContext);
            }
        }

        public bool IsEnabled(LogLevel logLevel)=>true;
    

        public IDisposable BeginScope<TState>(TState state)=> default!;
    }
}

