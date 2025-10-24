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
using Moq;
using SkyApm.Config;
using SkyApm.Utilities.Logging;
using Xunit;
using Serilog;

namespace SkyApm.Utilities.Logging.Tests
{
    public class DefaultLoggerFactoryTests
    {
        [Fact]
        public void CreateLoggerFactory_ShouldNotThrow_WithValidConfig()
        {
            // Arrange
            var configAccessor = new Mock<IConfigAccessor>();
            var loggingConfig = new LoggingConfig
            {
                Level = "Information",
                FilePath = "test.log",
                FileSizeLimitBytes = 1024 * 1024 * 10,
                FlushToDiskInterval = 1000,
                RollingInterval = "Day",
                RollOnFileSizeLimit = false,
                RetainedFileCountLimit = 7,
                RetainedFileTimeLimit = 1000 * 60 * 60 * 24 * 7
            };
            var instrumentConfig = new InstrumentConfig
            {
                ServiceName = "TestService"
            };

            configAccessor.Setup(x => x.Get<LoggingConfig>()).Returns(loggingConfig);
            configAccessor.Setup(x => x.Get<InstrumentConfig>()).Returns(instrumentConfig);

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                var factory = new DefaultLoggerFactory(configAccessor.Object);
                var logger = factory.CreateLogger(typeof(DefaultLoggerFactoryTests));
                Assert.NotNull(logger);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void CreateLogger_ShouldReturnValidLogger()
        {
            // Arrange
            var configAccessor = new Mock<IConfigAccessor>();
            var loggingConfig = new LoggingConfig
            {
                Level = "Debug",
                FilePath = "test.log"
            };
            var instrumentConfig = new InstrumentConfig
            {
                ServiceName = "TestService"
            };

            configAccessor.Setup(x => x.Get<LoggingConfig>()).Returns(loggingConfig);
            configAccessor.Setup(x => x.Get<InstrumentConfig>()).Returns(instrumentConfig);

            var factory = new DefaultLoggerFactory(configAccessor.Object);

            // Act
            var logger = factory.CreateLogger(typeof(DefaultLoggerFactoryTests));

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public void SerilogSeqConfiguration_ShouldNotThrow_WithValidConfig()
        {
            // Arrange - Test the specific configuration from issue #602
            var appSettings = new
            {
                SeqServer = "http://localhost:5341",
                SeqApiKey = "test-api-key"
            };

            // Act & Assert - This should not throw MissingMethodException
            var exception = Record.Exception(() =>
            {
                var logConfig = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Async(a => a.Seq(
                        serverUrl: appSettings.SeqServer,
                        apiKey: appSettings.SeqApiKey
                    ))
                    .CreateLogger();

                Assert.NotNull(logConfig);
            });

            Assert.Null(exception);
        }
    }
}