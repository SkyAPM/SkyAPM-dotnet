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

namespace SkyApm.Config
{
    public static class ProtocolVersions
    {
        public static string V8 { get; } = "v8";
    }

    [Config("SkyWalking", "Transport")]
    public class TransportConfig
    {
        public string ProtocolVersion { get; set; } = ProtocolVersions.V8;

        public int QueueSize { get; set; } = 80000;

        /// <summary>
        /// Flush Interval (Millisecond)
        /// </summary>
        public int Interval { get; set; } = 2000;

        /// <summary>
        /// Data queued beyond this throttle will be discarded.
        /// </summary>
        public int BatchSize { get; set; } = 2000;

        public int Parallel { get; set; } = 10;

        /// <summary>
        /// in milliseconds
        /// </summary>
        public int Pause { get; set; } = 500;
    }
}
