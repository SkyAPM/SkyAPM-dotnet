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
using SkyApm.Tracing.Segments;
using CapEvents = DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames;

namespace SkyApm.Diagnostics.CAP
{
    /// <summary>
    ///  Diagnostics processor for listen and process events of CAP.
    /// </summary>
    public class CapTracingDiagnosticProcessor : BaseCapDiagnosticProcessor, ICapDiagnosticProcessor
    {
        private readonly ConcurrentDictionary<string, SegmentContext> _contexts = new ConcurrentDictionary<string, SegmentContext>();
        public string ListenerName => CapEvents.DiagnosticListenerName;

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;

        public CapTracingDiagnosticProcessor(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            _contexts[eventData.Message.GetId()] = _entrySegmentContextAccessor.Context;

            var operationName = GetBeforePublishStoreOpName(eventData);
            var context = _tracingContext.CreateLocalSegmentContext(operationName);
            BeforePublishStoreSetupSpan(context.Span, eventData);
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null) return;

            AfterPublishStoreSetupSpan(context.Span, eventData);

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null) return;

            ErrorPublishStoreSetupSpan(_tracingConfig, context.Span, eventData);
            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            _localSegmentContextAccessor.Context = _contexts[eventData.TransportMessage.GetId()];

            var host = GetHost(eventData);
            var operationName = GetBeforePublishOpName(eventData);
            var context = _tracingContext.CreateExitSegmentContext(operationName, host, new CapCarrierHeaderCollection(eventData.TransportMessage));

            BeforePublishSetupSpan(context.Span, eventData, host);
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            AfterPublishSetupSpan(context.Span, eventData);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.TransportMessage.GetId(), out _);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            ErrorPublishSetupSpan(_tracingConfig, context.Span, eventData);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.TransportMessage.GetId(), out _);
        }

        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            var carrierHeader = new CapCarrierHeaderCollection(eventData.TransportMessage);
            var operationName = GetCapBeforeConsumeOpName(eventData);
            var context = _tracingContext.CreateEntrySegmentContext(operationName, carrierHeader);

            CapBeforeConsumeSetupSpan(context.Span, eventData);

            _contexts[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()] = context;
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _contexts[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()];
            if (context == null) return;

            CapAfterConsumeSetupSpan(context.Span, eventData);

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _contexts[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()];
            if (context == null) return;

            CapErrorConsumeSetupSpan(_tracingConfig, context.Span, eventData);

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            _entrySegmentContextAccessor.Context = _contexts[eventData.Message.GetId() + eventData.Message.GetGroup()];

            var operationName = GetCapBeforeSubscriberInvokeOpName(eventData);
            var context = _tracingContext.CreateLocalSegmentContext(operationName);
            CapBeforeSubscriberInvokeSetupSpan(context.Span, eventData);

            _contexts[eventData.Message.GetId() + eventData.Message.GetGroup()] = context;
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _contexts[eventData.Message.GetId() + eventData.Message.GetGroup()];
            if (context == null) return;

            CapAfterSubscriberInvokeSetupSpan(context.Span, eventData);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _contexts[eventData.Message.GetId() + eventData.Message.GetGroup()];
            if (context == null) return;

            CapErrorSubscriberInvokeSetupSpan(_tracingConfig, context.Span, eventData);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }
    }
}