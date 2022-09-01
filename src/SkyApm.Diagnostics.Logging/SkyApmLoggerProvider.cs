using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SkyApm.Tracing;
using SkyApm.Transport;

namespace SkyApm.Diagnostics.Logging
{
    public class SkyApmLoggerProvider: ILoggerProvider
    {
        private ConcurrentDictionary<string, SkyApmLogger> _doveLoggers = new ConcurrentDictionary<string, SkyApmLogger>();
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        public SkyApmLoggerProvider(ISkyApmLogDispatcher skyApmLogDispatcher,IEntrySegmentContextAccessor entrySegmentContextAccessor)
        {
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            
        }

        public void Dispose()
        {
        
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _doveLoggers.GetOrAdd(categoryName,_=>new SkyApmLogger(categoryName,_skyApmLogDispatcher,_entrySegmentContextAccessor));
        }
    }
}

