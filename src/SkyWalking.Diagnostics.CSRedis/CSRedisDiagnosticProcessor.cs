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
using CSRedis.NetCore.Diagnostics;
using SkyWalking.Context;
using SkyWalking.Context.Tag;
using SkyWalking.Context.Trace;
using SkyWalking.NetworkProtocol.Trace;
using CSRedisEvents = CSRedis.NetCore.Diagnostics.CSRedisDiagnosticListenerExtensions;


namespace SkyWalking.Diagnostics.CSRedis
{
    /// <summary>
    ///  Diagnostics processor for listen and process releted events of CSRedis.
    /// </summary>
    public class CSRedisDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private Func<BrokerPublishEventData, string> _brokerOperationNameResolver;

        public string ListenerName => CSRedisEvents.DiagnosticListenerName;

        public Func<BrokerPublishEventData, string> BrokerOperationNameResolver
        {
            get
            {
                return _brokerOperationNameResolver ??
                       (_brokerOperationNameResolver = (data) => "CSRedis " + data.Operation);
            }
            set => _brokerOperationNameResolver = value ?? throw new ArgumentNullException(nameof(BrokerOperationNameResolver));
        }

        [DiagnosticName(CSRedisEvents.CSRedisBeforePublishMessageStore)]
        public void CSRedisBeforePublish([Object]BrokerPublishEventData eventData)
        {
            var operationName = BrokerOperationNameResolver(eventData);
            var contextCarrier = new ContextCarrier();
            var peer = eventData.Address;
            var span = ContextManager.CreateExitSpan(operationName, contextCarrier, peer);
            span.SetComponent(ComponentsDefine.StackExchange_Redis);
            span.AsCache();
            Tags.DbType.Set(span, "Redis");
            Tags.DbStatement.Set(span, eventData.Content);
            //span.SetLayer(SpanLayer.CACHE);
        }

        [DiagnosticName(CSRedisEvents.CSRedisAfterPublishMessageStore)]
        public void CSRedisAfterPublish([Object]BrokerPublishEndEventData eventData)
        {
            ContextManager.StopSpan();
        }

        [DiagnosticName(CSRedisEvents.CSRedisErrorPublishMessageStore)]
        public void CSRedisErrorPublish([Object]BrokerPublishErrorEventData eventData)
        {
            var CSRedisSpan = ContextManager.ActiveSpan;
            if (CSRedisSpan == null)
            {
                return;
            }
            CSRedisSpan.Log(eventData.Exception);
            CSRedisSpan.ErrorOccurred();
            ContextManager.StopSpan(CSRedisSpan);
        }

    }
}
