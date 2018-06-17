/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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
using System.Threading.Tasks;
using Nito.AsyncEx;
using SkyWalking.AspNet.Logging;
using SkyWalking.Boot;
using SkyWalking.Logging;
using SkyWalking.Remote;

namespace SkyWalking.AspNet
{
    public class SkyWalkingStartup
    {
        public void Start()
        {
            LogManager.SetLoggerFactory(new DebugLoggerFactoryAdapter());
            AsyncContext.Run(async () => await StartAsync());
        }

        private async Task StartAsync()
        {
            await GrpcConnectionManager.Instance.ConnectAsync(TimeSpan.FromSeconds(3));
            await ServiceManager.Instance.Initialize();
        }
    }
}
