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

using MassTransit;
using System;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    public class MasstransitBusObserver : IBusObserver
    {
        private readonly IPublishObserver publishObserver;
        private readonly ISendObserver sendObserver;

        public MasstransitBusObserver(IPublishObserver publishObserver, ISendObserver sendObserver)
        {
            this.publishObserver = publishObserver;
            this.sendObserver = sendObserver;
        }


        public void CreateFaulted(Exception exception)
        {
            return;
        }

        public void PostCreate(IBus bus)
        {
            bus.ConnectPublishObserver(this.publishObserver);
            bus.ConnectSendObserver(this.sendObserver);
        }

        public Task PostStart(IBus bus, Task<BusReady> busReady)
        {
            return Task.CompletedTask;
        }

        public Task PostStop(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task PreStart(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task PreStop(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task StartFaulted(IBus bus, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task StopFaulted(IBus bus, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
