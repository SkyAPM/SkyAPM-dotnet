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

using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class LoggerContextContextMapper : ILoggerContextContextMapper
    {
        private readonly ISegmentContextMapper _segmentContextMapper;

        public LoggerContextContextMapper(ISegmentContextMapper segmentContextMapper)
        {
            _segmentContextMapper = segmentContextMapper;
        }

        public LoggerRequest Map(LoggerContext loggerContext)
        {
            var segmentRequest = _segmentContextMapper.Map(loggerContext.SegmentContext);
            return new LoggerRequest
            {
                Logs = loggerContext.Logs,
                SegmentRequest = segmentRequest,
                Date = loggerContext.Date,
            };
        }
    }
}
