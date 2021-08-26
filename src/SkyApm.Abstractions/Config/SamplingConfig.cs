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

using System.Collections.Generic;

namespace SkyApm.Config
{
    [Config("SkyWalking", "Sampling")]
    public class SamplingConfig
    {
        public int SamplePer3Secs { get; set; } = -1;

        public double Percentage { get; set; } = -1d;

        /// <summary>
        /// Paths to ignore, support wildchar match.
        /// Usage: a/b/c => a/b/c, a/* => a/b, a/** => a/b/c/d, a/?/c => a/b/c
        /// </summary>
        public List<string> IgnorePaths { get; set; }

        /// <summary>
        /// whether or not  to record the value of sql parameter.
        /// true:record parameter value 
        /// </summary>
        public bool LogSqlParameterValue { get; set; }
    }
}