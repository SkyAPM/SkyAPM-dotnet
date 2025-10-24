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

using SkyApm.Config;
using Xunit;

namespace SkyApm.Core.Tests
{
    public class GrpcConfigTests
    {
        [Fact]
        public void ShouldEnableSSL_DefaultConfig_ReturnsFalse()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800"
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldEnableSSL_EnableSSLSetToTrue_ReturnsTrue()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800",
                EnableSSL = true
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldEnableSSL_EnableSSLSetToFalse_ReturnsFalse()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800",
                EnableSSL = false
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldEnableSSL_HttpsServer_ReturnsTrue()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "https://skywalking-oap:11800"
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldEnableSSL_HttpsServerWithExplicitFalse_ReturnsFalse()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "https://skywalking-oap:11800",
                EnableSSL = false
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldEnableSSL_MixedServersWithHttps_ReturnsTrue()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800,https://skywalking-oap:11800"
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldEnableSSL_MultipleHttpsServers_ReturnsTrue()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "https://server1:11800,https://server2:11800"
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldEnableSSL_NullServers_ReturnsFalse()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = null
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldEnableSSL_EmptyServers_ReturnsFalse()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = ""
            };

            // Act
            var result = config.ShouldEnableSSL();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetServers_DefaultConfig_AddsHttpPrefix()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800"
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Single(result);
            Assert.Equal("http://localhost:11800", result[0]);
        }

        [Fact]
        public void GetServers_EnableSSL_AddsHttpsPrefix()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800",
                EnableSSL = true
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Single(result);
            Assert.Equal("https://localhost:11800", result[0]);
        }

        [Fact]
        public void GetServers_HttpsServer_KeepsHttpsPrefix()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "https://skywalking-oap:11800"
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Single(result);
            Assert.Equal("https://skywalking-oap:11800", result[0]);
        }

        [Fact]
        public void GetServers_DnsScheme_KeepsDnsPrefix()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "dns://skywalking-oap:11800"
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Single(result);
            Assert.Equal("dns://skywalking-oap:11800", result[0]);
        }

        [Fact]
        public void GetServers_MultipleServers_AddsPrefixesCorrectly()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800,dns://skywalking-oap:11800,https://secure-server:11800"
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal("http://localhost:11800", result[0]);
            Assert.Equal("dns://skywalking-oap:11800", result[1]);
            Assert.Equal("https://secure-server:11800", result[2]);
        }

        [Fact]
        public void GetServers_MultipleServersWithHttps_AddsHttpsPrefixes()
        {
            // Arrange
            var config = new GrpcConfig
            {
                Servers = "localhost:11800,skywalking-oap:11800",
                EnableSSL = true
            };

            // Act
            var result = config.GetServers();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal("https://localhost:11800", result[0]);
            Assert.Equal("https://skywalking-oap:11800", result[1]);
        }
    }
}