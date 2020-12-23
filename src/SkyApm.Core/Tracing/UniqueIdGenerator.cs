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
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SkyApm.Tracing
{
    public class UniqueIdGenerator : IUniqueIdGenerator
    {
        private readonly ThreadLocal<long> sequence = new ThreadLocal<long>(() => 0);
        private readonly InstrumentConfig _instrumentConfig;
        private readonly string _instanceIdentity;

        public UniqueIdGenerator(IConfigAccessor configAccessor)
        {
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _instanceIdentity = GetMD5(_instrumentConfig.ServiceInstanceName);
        }

        public string Generate()
        {
            var part1 = _instanceIdentity;
            var part2 = Thread.CurrentThread.ManagedThreadId;
            var part3 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10000 + GetSequence();
            return $"{part1}.{part2}.{part3}";
        }

        private string GetMD5(string data)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                var sb = new StringBuilder(32);
                foreach (var item in hash)
                {
                    sb.Append(item.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private long GetSequence()
        {
            if (sequence.Value++ >= 9999)
            {
                sequence.Value = 0;
            }

            return sequence.Value;
        }
    }
}