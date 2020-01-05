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

using System.Collections.Concurrent;
using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using CapEvents = DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames;

namespace SkyApm.Diagnostics.CAP
{
    /// <summary>
    ///  Diagnostics processor for listen and process events of CAP.
    /// </summary>
    public class CapTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private readonly ConcurrentDictionary<string, SegmentContext> _contexts = new ConcurrentDictionary<string, SegmentContext>();
        public string ListenerName => CapEvents.DiagnosticListenerName;

        private const string OperateNamePrefix = "CAP/";
        private const string ProducerOperateNameSuffix = "/Publisher";
        private const string ConsumerOperateNameSuffix = "/Subscriber";

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;

        public CapTracingDiagnosticProcessor(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor)
        {
            _tracingContext = tracingContext;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
        }

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            _contexts[eventData.Message.GetId()] = _entrySegmentContextAccessor.Context;

            var context = _tracingContext.CreateLocalSegmentContext("Event Persistence: " + eventData.Operation);
            //context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = Components.CAP;
            //context.Span.AddTag(Tags.DB_TYPE, "sql");
            context.Span.AddLog(LogEvent.Event("Event Persistence"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence start..."));
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence succeeded!"));
            context.Span.AddLog(LogEvent.Message("Spend Time: " + eventData.ElapsedTimeMs + "ms"));

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubSend eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.ErrorOccurred(eventData.Exception);
            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            _localSegmentContextAccessor.Context = _contexts[eventData.TransportMessage.GetId()];

            var context = _tracingContext.CreateExitSegmentContext(OperateNamePrefix + eventData.Operation + ProducerOperateNameSuffix,
                eventData.BrokerAddress.Endpoint.Replace("-1", "5672"),
                new CapCarrierHeaderCollection(eventData.TransportMessage));

            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = GetComponent(eventData.BrokerAddress, true);

            context.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            context.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            context.Span.AddLog(LogEvent.Event("Event Publishing"));
            context.Span.AddLog(LogEvent.Message("Message publishing start..."));
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Publishing"));
            context.Span.AddLog(LogEvent.Message("Message publishing succeeded!"));
            context.Span.AddLog(LogEvent.Message("Spend Time: " + eventData.ElapsedTimeMs + "ms"));

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.TransportMessage.GetId(), out _);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Publishing"));
            context.Span.AddLog(LogEvent.Message("Message publishing failed!"));
            context.Span.ErrorOccurred(eventData.Exception);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.TransportMessage.GetId(), out _);
        }


        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            var carrierHeader = new CapCarrierHeaderCollection(eventData.TransportMessage);
            var context = _tracingContext.CreateEntrySegmentContext(OperateNamePrefix + eventData.Operation + ConsumerOperateNameSuffix, carrierHeader);
            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = GetComponent(eventData.BrokerAddress, false);
            context.Span.Peer = eventData.BrokerAddress.Endpoint;
            context.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            context.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            context.Span.AddLog(LogEvent.Event("Event Persistence"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence start..."));

            _contexts[eventData.TransportMessage.GetId()] = _entrySegmentContextAccessor.Context;
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence succeeded!"));
            context.Span.AddLog(LogEvent.Message("Persistence spend time: " + eventData.ElapsedTimeMs + "ms"));

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence"));
            context.Span.AddLog(LogEvent.Message("Message persistence failed!"));
            context.Span.ErrorOccurred(eventData.Exception);

            _tracingContext.Release(context);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            _entrySegmentContextAccessor.Context = _contexts[eventData.Message.GetId()];

            var context = _tracingContext.CreateLocalSegmentContext("Subscriber Invoke: " + eventData.MethodInfo.Name);
            //context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = Components.CAP;
            //context.Span.AddTag(Tags.DB_TYPE, "Sql");
            context.Span.AddLog(LogEvent.Event("Subscriber Invoke"));
            context.Span.AddLog(LogEvent.Message("Begin invoke the subscriber: " + eventData.MethodInfo.Name));
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Subscriber Invoke"));
            context.Span.AddLog(LogEvent.Message("Subscriber invoke succeeded!"));
            context.Span.AddLog(LogEvent.Message("Subscriber invoke spend time: " + eventData.ElapsedTimeMs + "ms"));

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.Message.GetId(), out _);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Subscriber Invoke"));
            context.Span.AddLog(LogEvent.Message("Subscriber invoke failed!"));
            context.Span.ErrorOccurred(eventData.Exception);

            _tracingContext.Release(context);

            _contexts.TryRemove(eventData.Message.GetId(), out _);
        }

        private StringOrIntValue GetComponent(BrokerAddress address, bool isPub)
        {
            if (isPub)
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return "rabbitmq-producer";
                    case "Kafka":
                        return "kafka-producer";
                }
            }
            else
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return "rabbitmq-consumer";
                    case "Kafka":
                        return "kafka-consumer";
                }
            }
            return Components.CAP;
        }
    }
}