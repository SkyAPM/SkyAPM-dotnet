using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Newtonsoft.Json;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;

namespace SkyApm.Diagnostics.CAP
{
    public abstract class BaseCapDiagnosticProcessor
    {
        protected const string OperateNamePrefix = "CAP/";
        protected const string ProducerOperateNameSuffix = "/Publisher";
        protected const string ConsumerOperateNameSuffix = "/Subscriber";

        protected string GetBeforePublishStoreOpName(CapEventDataPubStore eventData) => "Event Persistence: " + eventData.Operation;

        protected void BeforePublishStoreSetupSpan(SegmentSpan span, CapEventDataPubStore eventData)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = Components.CAP;
            span.AddTag(Tags.DB_TYPE, "Sql");
            span.AddLog(LogEvent.Event("Event Persistence Start"));
            span.AddLog(LogEvent.Message("CAP message persistence start..."));
        }

        protected void AfterPublishStoreSetupSpan(SegmentSpan span, CapEventDataPubStore eventData)
        {
            span.AddLog(LogEvent.Event("Event Persistence End"));
            span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.{Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId() } , Name: { eventData.Operation} "));
        }

        protected void ErrorPublishStoreSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CapEventDataPubStore eventData)
        {
            span.AddLog(LogEvent.Event("Event Persistence Error"));
            span.AddLog(LogEvent.Message($"CAP message persistence failed!{Environment.NewLine}" +
                                                 $"--> Message Info:{Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected string GetHost(CapEventDataPubSend eventData) => eventData.BrokerAddress.Endpoint.Replace("-1", "5672");

        protected string GetBeforePublishOpName(CapEventDataPubSend eventData) => OperateNamePrefix + eventData.Operation + ProducerOperateNameSuffix;

        protected void BeforePublishSetupSpan(SegmentSpan span, CapEventDataPubSend eventData, string host)
        {
            span.SpanLayer = SpanLayer.MQ;
            span.Component = GetComponent(eventData.BrokerAddress, true);
            span.Peer = host;
            span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            span.AddLog(LogEvent.Event("Event Publishing Start"));
            span.AddLog(LogEvent.Message("CAP message publishing start..."));
        }

        protected void AfterPublishSetupSpan(SegmentSpan span, CapEventDataPubSend eventData)
        {
            span.AddLog(LogEvent.Event("Event Publishing End"));
            span.AddLog(LogEvent.Message($"CAP message publishing succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));
        }

        protected void ErrorPublishSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CapEventDataPubSend eventData)
        {
            span.AddLog(LogEvent.Event("Event Publishing Error"));
            span.AddLog(LogEvent.Message($"CAP message publishing failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));
            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected string GetCapBeforeConsumeOpName(CapEventDataSubStore eventData)
        {
            var eventName = eventData.TransportMessage.GetGroup() + "/" + eventData.Operation;
            return OperateNamePrefix + eventName + ConsumerOperateNameSuffix;
        }

        protected void CapBeforeConsumeSetupSpan(SegmentSpan span, CapEventDataSubStore eventData)
        {
            span.SpanLayer = SpanLayer.DB;
            span.Component = GetComponent(eventData.BrokerAddress, false);
            span.Peer = eventData.BrokerAddress.Endpoint.Replace("-1", "5672");
            span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            span.AddLog(LogEvent.Event("Event Persistence Start"));
            span.AddLog(LogEvent.Message("CAP message persistence start..."));
        }

        protected void CapAfterConsumeSetupSpan(SegmentSpan span, CapEventDataSubStore eventData)
        {
            span.AddLog(LogEvent.Event("Event Persistence End"));
            span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms. {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));
        }

        protected void CapErrorConsumeSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CapEventDataSubStore eventData)
        {
            span.AddLog(LogEvent.Event("Event Persistence Error"));
            span.AddLog(LogEvent.Message($"CAP message publishing failed! {Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));
            span.ErrorOccurred(eventData.Exception, tracingConfig);
        }

        protected string GetCapBeforeSubscriberInvokeOpName(CapEventDataSubExecute eventData) => "Subscriber Invoke: " + eventData.MethodInfo.Name;

        protected void CapBeforeSubscriberInvokeSetupSpan(SegmentSpan span, CapEventDataSubExecute eventData)
        {
            span.SpanLayer = SpanLayer.MQ;
            span.Component = Components.CAP;
            span.AddLog(LogEvent.Event("Subscriber Invoke Start"));
            span.AddLog(LogEvent.Message($"Begin invoke the subscriber: {eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId()}, Group: {eventData.Message.GetGroup()}, Name: {eventData.Operation}"));
        }

        protected void CapAfterSubscriberInvokeSetupSpan(SegmentSpan span, CapEventDataSubExecute eventData)
        {
            span.AddLog(LogEvent.Event("Subscriber Invoke End"));
            span.AddLog(LogEvent.Message("Subscriber invoke succeeded!"));
            span.AddLog(LogEvent.Message($"Subscriber invoke spend time: { eventData.ElapsedTimeMs}ms. {Environment.NewLine}" +
                                                 $"--> Method Info: {eventData.MethodInfo}"));
        }

        protected void CapErrorSubscriberInvokeSetupSpan(TracingConfig tracingConfig, SegmentSpan span, CapEventDataSubExecute eventData)
        {
            span.AddLog(LogEvent.Event("Subscriber Invoke Error"));
            span.AddLog(LogEvent.Message($"Subscriber invoke failed! {Environment.NewLine}" +
                                                 $"--> Method Info: { eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Info: {Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            span.ErrorOccurred(eventData.Exception, tracingConfig);
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
