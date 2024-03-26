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

using System;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using SkyApm.Config;
using ILogger = SkyApm.Logging.ILogger;
using ILoggerFactory = SkyApm.Logging.ILoggerFactory;
using MSLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;

namespace SkyApm.Utilities.Logging
{
    public class DefaultLoggerFactory : SkyApm.Logging.ILoggerFactory
    {
        private const string outputTemplate =
            @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{ServiceName}] [{Level}] {SourceContext} : {Message}{NewLine}{Exception}";

        private readonly MSLoggerFactory _loggerFactory;
        private readonly LoggingConfig _loggingConfig;

        public DefaultLoggerFactory(IConfigAccessor configAccessor)
        {
            _loggingConfig = configAccessor.Get<LoggingConfig>();
            _loggerFactory = new MSLoggerFactory();
            var instrumentationConfig = configAccessor.Get<InstrumentConfig>();

            var __level = EventLevel(_loggingConfig.Level);
            long __fileSizeLimitBytes = _loggingConfig.FileSizeLimitBytes ?? 1024 * 1024 * 256;
            long __flushToDiskInterval = _loggingConfig.FlushToDiskInterval ?? 1000;
            string __rollingInterval = _loggingConfig.RollingInterval ?? "Day";
            bool __rollOnFileSizeLimit = _loggingConfig.RollOnFileSizeLimit ?? false;
            int __retainedFileCountLimit = _loggingConfig.RetainedFileCountLimit ?? 10;
            long __retainedFileTimeLimit = _loggingConfig.RetainedFileTimeLimit ?? 1000 * 60 * 60 * 24 * 10;

            _loggerFactory.AddSerilog(new LoggerConfiguration().MinimumLevel.Verbose().Enrich
                .WithProperty("SourceContext", null).Enrich
                .WithProperty(nameof(instrumentationConfig.ServiceName), instrumentationConfig.ServiceName).Enrich
                .FromLogContext()
                .WriteTo
                .Async(o => o.File(
                    _loggingConfig.FilePath,
                    __level,
                    outputTemplate,
                    fileSizeLimitBytes: __fileSizeLimitBytes,
                    flushToDiskInterval: TimeSpan.FromMilliseconds(__flushToDiskInterval),
                    rollingInterval: (RollingInterval)(Enum.Parse(typeof(RollingInterval), __rollingInterval)),
                    rollOnFileSizeLimit: __rollOnFileSizeLimit,
                    retainedFileCountLimit: __retainedFileCountLimit,
                    retainedFileTimeLimit: TimeSpan.FromMilliseconds(__retainedFileTimeLimit)))
                .CreateLogger());
        }

        public SkyApm.Logging.ILogger CreateLogger(Type type)
        {
            return new DefaultLogger(_loggerFactory.CreateLogger(type));
        }

        private static LogEventLevel EventLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, out var logEventLevel)
                ? logEventLevel
                : LogEventLevel.Error;
        }
    }
}