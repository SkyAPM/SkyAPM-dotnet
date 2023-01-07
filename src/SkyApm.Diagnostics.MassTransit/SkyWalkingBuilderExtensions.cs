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
using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.MassTransit.Common;
using SkyApm.Diagnostics.MassTransit.Observers;
using SkyApm.Utilities.DependencyInjection;
using System;

namespace SkyApm.Diagnostics.MassTransit
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddMasstransit(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }
 
            // Masstransit >= 8.0.0
            extensions.Services.AddSingleton<IBusObserver, MasstransitBusObserver>();;
            extensions.Services.AddSingleton<IPublishObserver, MasstransitPublishObserver>();
            extensions.Services.AddSingleton<IReceiveObserver, MasstransitReceiveObserver>();
            extensions.Services.AddSingleton<IConsumeObserver, MasstransitConsumerObserver>();
            extensions.Services.AddSingleton<ISendObserver, MasstransitPublishObserver>();
            //AddComponentIdChecker
            extensions.AddComponentIdChecker();
            // Masstransit < 8.0.0
            // Need to be done?
            return extensions;
        }

        /// <summary>
        /// Please check component-libraries.yml for more details
        /// https://github.com/apache/skywalking/blob/master/oap-server/server-starter/src/main/resources/component-libraries.yml
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns></returns>
        private static SkyApmExtensions AddComponentIdChecker(this SkyApmExtensions extensions)
        {
            extensions.Services.AddSingleton<IGetComponentUtil, GetComponentUtil>();
            extensions.Services.AddSingleton<IComponentIdChecker, RabbitmqComponentIdChecker>();
            return extensions;
        }
    }
}