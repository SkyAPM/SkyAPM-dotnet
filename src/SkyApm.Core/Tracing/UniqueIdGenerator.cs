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

using SkyApm.Common;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SkyApm.Tracing
{
    public class UniqueIdGenerator : IUniqueIdGenerator
    {
        private readonly ThreadLocal<long> sequence = new ThreadLocal<long>(() => 0);
        private readonly IRuntimeEnvironment _runtimeEnvironment;

        public UniqueIdGenerator(IRuntimeEnvironment runtimeEnvironment)
        {
            _runtimeEnvironment = runtimeEnvironment;
        }

        public UniqueId Generate()
        { 
            var instanceIdentity = _runtimeEnvironment.ServiceInstanceId.HasValue
                ? new StringOrNumValue<long>(_runtimeEnvironment.ServiceInstanceId.Value)
                : new StringOrNumValue<long>(_runtimeEnvironment.InstanceId.ToString("N"));

            return new UniqueId(instanceIdentity,
                Thread.CurrentThread.ManagedThreadId,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10000 + GetSequence());
        }

        private string GetMD5(byte[] data)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(data);
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