using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using SkyApm.Tracing;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.Logging.Serilog.Sinks
{
    public class SkywalkingSink : ILogEventSink
    {
        ITextFormatter _formatter;
        IServiceProvider _serviceCollection;

        public SkywalkingSink(IServiceProvider serviceCollection, ITextFormatter formatter)
        {
            _serviceCollection = serviceCollection;
            _formatter = formatter;
        }

        ISkyApmLogDispatcher _skyApmLogDispatcher;
        IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        object _locker = new object();

        public void Emit(LogEvent logEvent)
        {
            if (_skyApmLogDispatcher == null)
            {
                lock (_locker)
                {
                    if (_skyApmLogDispatcher == null)
                    {
                        _skyApmLogDispatcher = _serviceCollection.GetService<ISkyApmLogDispatcher>();
                        if (_skyApmLogDispatcher == null)
                            return;
                    }
                }
            }

            if (_entrySegmentContextAccessor == null)
            {
                lock (_locker)
                {
                    if (_entrySegmentContextAccessor == null)
                    {
                        _entrySegmentContextAccessor = _serviceCollection.GetService<IEntrySegmentContextAccessor>();
                        if (_entrySegmentContextAccessor == null)
                            return;
                    }
                }
            }

            if (_entrySegmentContextAccessor.Context == null)
            {
                return;
            }

            string renderMessage = null;
            if (_formatter != null)
            {
                using var render = new StringWriter(CultureInfo.InvariantCulture);
                _formatter.Format(logEvent, render);

                renderMessage = render.ToString();
            }
            else
            {
                renderMessage = logEvent.RenderMessage();

                if (logEvent.Exception != null)
                    renderMessage += Environment.NewLine + logEvent.Exception.ToString();
            }

            var logs = new Dictionary<string, object>();

            //logs.Add("className", "className");
            logs.Add("Level", logEvent.Level.ToString());
            logs.Add("logMessage", renderMessage);

            var logContext = new Tracing.Segments.LoggerContext()
            {
                Logs = logs,
                SegmentContext = _entrySegmentContextAccessor.Context,
                Date = DateTimeOffset.UtcNow.Offset.Ticks
            };

            _skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
