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

using System;
using System.Collections.Concurrent;
using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Messages;
using SkyApm.Config;
using SkyApm.Tracing;
using CapEvents = DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames;

namespace SkyApm.Diagnostics.CAP
{
    /// <summary>
    ///  Diagnostics processor for listen and process events of CAP.
    /// </summary>
    public class SpanCapTracingDiagnosticProcessor : BaseCapDiagnosticProcessor, ICapDiagnosticProcessor
    {
        private readonly ConcurrentDictionary<string, CrossThreadCarrier> _carriers = new ConcurrentDictionary<string, CrossThreadCarrier>();
        public string ListenerName => CapEvents.DiagnosticListenerName;

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public SpanCapTracingDiagnosticProcessor(ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            var operationName = GetBeforePublishStoreOpName(eventData);
            var span = _tracingContext.CreateLocalSpan(operationName);

            BeforePublishStoreSetupSpan(span, eventData);
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterPublishStoreSetupSpan(span, eventData);

            _carriers[eventData.Message.GetId()] = _tracingContext.StopSpanGetCarrier(span);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubStore eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            ErrorPublishStoreSetupSpan(_tracingConfig, span, eventData);

            _carriers[eventData.Message.GetId()] = _tracingContext.StopSpanGetCarrier(span);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            if (!_carriers.TryRemove(eventData.TransportMessage.GetId(), out var carrier)) return;

            var host = GetHost(eventData);
            var operationName = GetBeforePublishOpName(eventData);
            var span = _tracingContext.CreateExitSpan(operationName, host, carrier, new CapCarrierHeaderCollection(eventData.TransportMessage));

            BeforePublishSetupSpan(span, eventData, host);
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            AfterPublishSetupSpan(span, eventData);

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            ErrorPublishSetupSpan(_tracingConfig, span, eventData);

            _tracingContext.StopSpan(span);
        }


        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            var carrierHeader = new CapCarrierHeaderCollection(eventData.TransportMessage);
            var operationName = GetCapBeforeConsumeOpName(eventData);
            var span = _tracingContext.CreateEntrySpan(operationName, carrierHeader);

            CapBeforeConsumeSetupSpan(span, eventData);
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CapAfterConsumeSetupSpan(span, eventData);

            _carriers[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()] = _tracingContext.StopSpanGetCarrier(span);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CapErrorConsumeSetupSpan(_tracingConfig, span, eventData);

            _carriers[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()] = _tracingContext.StopSpanGetCarrier(span);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            if (!_carriers.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out var carrier)) return;

            var operationName = GetCapBeforeSubscriberInvokeOpName(eventData);
            var span = _tracingContext.CreateLocalSpan(operationName, carrier);

            CapBeforeSubscriberInvokeSetupSpan(span, eventData);
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CapAfterSubscriberInvokeSetupSpan(span, eventData);

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CapErrorSubscriberInvokeSetupSpan(_tracingConfig, span, eventData);

            _tracingContext.StopSpan(span);
        }
    }
}