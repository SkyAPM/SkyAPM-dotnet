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
using DotNetCore.CAP.Transport;
using Newtonsoft.Json;
using SkyApm.Common;
using SkyApm.Config;
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
        private readonly ConcurrentDictionary<string, CrossThreadCarrier> _carriers = new ConcurrentDictionary<string, CrossThreadCarrier>();
        public string ListenerName => CapEvents.DiagnosticListenerName;

        private const string OperateNamePrefix = "CAP/";
        private const string ProducerOperateNameSuffix = "/Publisher";
        private const string ConsumerOperateNameSuffix = "/Subscriber";

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public CapTracingDiagnosticProcessor(ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            var spanOrSegment = _tracingContext.CreateLocal("Event Persistence: " + eventData.Operation);
            spanOrSegment.Span.SpanLayer = SpanLayer.DB;
            spanOrSegment.Span.Component = Components.CAP;
            spanOrSegment.Span.AddTag(Tags.DB_TYPE, "Sql");
            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence Start"));
            spanOrSegment.Span.AddLog(LogEvent.Message("CAP message persistence start..."));

            _carriers[eventData.Message.GetId()] = spanOrSegment.GetCrossThreadCarrier();
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence End"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.{Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId() } , Name: { eventData.Operation} "));

            _tracingContext.Finish(spanOrSegment);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubStore eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence Error"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message persistence failed!{Environment.NewLine}" +
                                                 $"--> Message Info:{Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Finish(spanOrSegment);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            var carrier = _carriers[eventData.TransportMessage.GetId()];

            var host = eventData.BrokerAddress.Endpoint.Replace("-1", "5672");
            var spanOrSegment = _tracingContext.CreateExit(OperateNamePrefix + eventData.Operation + ProducerOperateNameSuffix,
                host, carrier, new CapCarrierHeaderCollection(eventData.TransportMessage));

            spanOrSegment.Span.SpanLayer = SpanLayer.MQ;
            spanOrSegment.Span.Component = GetComponent(eventData.BrokerAddress, true);
            spanOrSegment.Span.Peer = host;
            spanOrSegment.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            spanOrSegment.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            spanOrSegment.Span.AddLog(LogEvent.Event("Event Publishing Start"));
            spanOrSegment.Span.AddLog(LogEvent.Message("CAP message publishing start..."));
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            var spanOrSegment = _tracingContext.CurrentExit;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Publishing End"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message publishing succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));

            _tracingContext.Finish(spanOrSegment);

            _carriers.TryRemove(eventData.TransportMessage.GetId(), out _);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            var spanOrSegment = _tracingContext.CurrentExit;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Publishing Error"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message publishing failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));
            spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(spanOrSegment);

            _carriers.TryRemove(eventData.TransportMessage.GetId(), out _);
        }


        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            var carrierHeader = new CapCarrierHeaderCollection(eventData.TransportMessage);
            var eventName = eventData.TransportMessage.GetGroup() + "/" + eventData.Operation;
            var operationName = OperateNamePrefix + eventName + ConsumerOperateNameSuffix;
            var spanOrSegment = _tracingContext.CreateEntry(operationName, carrierHeader);
            spanOrSegment.Span.SpanLayer = SpanLayer.DB;
            spanOrSegment.Span.Component = GetComponent(eventData.BrokerAddress, false);
            spanOrSegment.Span.Peer = eventData.BrokerAddress.Endpoint.Replace("-1", "5672");
            spanOrSegment.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            spanOrSegment.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence Start"));
            spanOrSegment.Span.AddLog(LogEvent.Message("CAP message persistence start..."));

            _carriers[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()] = spanOrSegment.GetCrossThreadCarrier();
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            var spanOrSegment = _tracingContext.CurrentEntry;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence End"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms. {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));

            _tracingContext.Finish(spanOrSegment);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            var spanOrSegment = _tracingContext.CurrentEntry;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Event Persistence Error"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"CAP message publishing failed! {Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));
            spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(spanOrSegment);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var carrier = _carriers[eventData.Message.GetId() + eventData.Message.GetGroup()];

            var spanOrSegment = _tracingContext.CreateLocal("Subscriber Invoke: " + eventData.MethodInfo.Name, carrier);
            spanOrSegment.Span.SpanLayer = SpanLayer.MQ;
            spanOrSegment.Span.Component = Components.CAP;
            spanOrSegment.Span.AddLog(LogEvent.Event("Subscriber Invoke Start"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"Begin invoke the subscriber: {eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId()}, Group: {eventData.Message.GetGroup()}, Name: {eventData.Operation}"));
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Subscriber Invoke End"));
            spanOrSegment.Span.AddLog(LogEvent.Message("Subscriber invoke succeeded!"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"Subscriber invoke spend time: { eventData.ElapsedTimeMs}ms. {Environment.NewLine}" +
                                                 $"--> Method Info: {eventData.MethodInfo}"));

            _tracingContext.Finish(spanOrSegment);

            _carriers.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var spanOrSegment = _tracingContext.CurrentLocal;
            if (spanOrSegment == null) return;

            spanOrSegment.Span.AddLog(LogEvent.Event("Subscriber Invoke Error"));
            spanOrSegment.Span.AddLog(LogEvent.Message($"Subscriber invoke failed! {Environment.NewLine}" +
                                                 $"--> Method Info: { eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Info: {Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            spanOrSegment.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(spanOrSegment);

            _carriers.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }

        private StringOrIntValue GetComponent(BrokerAddress address, bool isPub)
        {
            if (isPub)
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return 52;  // "rabbitmq-producer";
                    case "Kafka":
                        return 40;  //"kafka-producer";
                }
            }
            else
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return 53; // "rabbitmq-consumer";
                    case "Kafka":
                        return 41; // "kafka-consumer";
                }
            }
            return Components.CAP;
        }
    }
}