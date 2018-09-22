using System;
using Serilog;
using Serilog.Events;
using SkyWalking.Config;
using Microsoft.Extensions.Logging;
using ILogger = SkyWalking.Logging.ILogger;
using ILoggerFactory = SkyWalking.Logging.ILoggerFactory;
using MSLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;

namespace SkyWalking.Extensions.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private const string outputTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{ApplicationCode}] [{Level}] {SourceContext} : {Message}{NewLine}{Exception}";
        private readonly MSLoggerFactory _loggerFactory;
        private readonly LoggingConfig _loggingConfig;

        public DefaultLoggerFactory(IConfigAccessor configAccessor)
        {
            _loggingConfig = configAccessor.Get<LoggingConfig>();
            _loggerFactory = new MSLoggerFactory();
            var instrumentationConfig = configAccessor.Get<InstrumentationConfig>();

            var level = EventLevel(_loggingConfig.Level);

            _loggerFactory.AddSerilog(new LoggerConfiguration().
                MinimumLevel.Verbose().
                Enrich.WithProperty("SourceContext", null).
                Enrich.WithProperty(nameof(instrumentationConfig.ApplicationCode), instrumentationConfig.ApplicationCode).
                Enrich.FromLogContext().
                WriteTo.RollingFile(_loggingConfig.FilePath, level, outputTemplate, null, 1073741824, 31, null, false, false, TimeSpan.FromMilliseconds(500)).
                CreateLogger());
        }

        public ILogger CreateLogger(Type type)
        {
            return new DefaultLogger(_loggerFactory.CreateLogger(type));
        }

        private static LogEventLevel EventLevel(string level)
        {
            return LogEventLevel.TryParse<LogEventLevel>(level, out var logEventLevel) ? logEventLevel : LogEventLevel.Error;
        }
    }
}