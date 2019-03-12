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
using System.IO;
using Serilog;
using Serilog.Events;
using SkyApm.Config;
using ILogger = SkyApm.Logging.ILogger;
using ILoggerFactory = SkyApm.Logging.ILoggerFactory;

namespace SkyApm.ClrProfiler.Trace.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private const string outputTemplate =
            @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{ServiceName}] [{Level}] {SourceContext} : {Message}{NewLine}{Exception}";

        private readonly DefaultLogger _logger;

        public DefaultLoggerFactory(IConfigAccessor configAccessor, TraceEnvironment traceEnvironment)
        {
            var instrumentationConfig = configAccessor.Get<InstrumentConfig>();
            var logger = new LoggerConfiguration().MinimumLevel.Verbose()
             .Enrich
             .WithProperty("SourceContext", null).Enrich
             .WithProperty(nameof(instrumentationConfig.ServiceName), instrumentationConfig.ServiceName)
             .Enrich
             .FromLogContext()
             .WriteTo
             .RollingFile(Path.Combine(traceEnvironment.GetProfilerHome(), "logs"), LogEventLevel.Error,
                          outputTemplate, null, 1073741824, 31,
                          null, false, false, TimeSpan.FromMilliseconds(500)).CreateLogger();

            _logger = new DefaultLogger(logger);
        }

        public ILogger CreateLogger(Type type)
        {
            return _logger;
        }

        private class DefaultLogger : ILogger
        {
            private readonly Serilog.Core.Logger _logger;

            public DefaultLogger(Serilog.Core.Logger logger)
            {
                _logger = logger;
            }

            public void Debug(string message)
            {
                _logger.Debug(message);
            }

            public void Information(string message)
            {
                _logger.Information(message);
            }

            public void Warning(string message)
            {
                _logger.Warning(message);
            }

            public void Error(string message, Exception exception)
            {
                _logger.Error(exception, message);
            }

            public void Trace(string message)
            {
            }
        }
    }
}