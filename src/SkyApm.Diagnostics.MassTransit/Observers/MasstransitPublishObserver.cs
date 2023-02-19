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
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.MassTransit.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    /// <summary>
    /// Publishing and Sending endpoint has same logic.
    /// </summary>
    public class MasstransitPublishObserver : IPublishObserver, ISendObserver
    {
        private readonly ConcurrentDictionary<Guid, SegmentContext> _contexts = new ConcurrentDictionary<Guid, SegmentContext>();
        private const string OperateNamePrefix = "Masstransit Publishing/";

        private readonly ITracingContext _tracingContext;
        private IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly IGetComponentUtil _getComponentID;
        private IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private TracingConfig _tracingConfig;

        public MasstransitPublishObserver(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            IGetComponentUtil getComponentID,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            this._getComponentID = getComponentID;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }
        public Task PrePublish<T>(PublishContext<T> pubContext) where T : class
        {
            return PreHandleSendEndpoint(pubContext);
        }

        public Task PreSend<T>(SendContext<T> sendContext) where T : class
        {
            return PreHandleSendEndpoint(sendContext, "Masstransit Sending/");
        }

        public Task PostPublish<T>(PublishContext<T> pubContext) where T : class
        {
            return PostHandleSendEndpoint(pubContext);
        }

        public Task PostSend<T>(SendContext<T> sendContext) where T : class
        {
            return PostHandleSendEndpoint(sendContext);
        }

        public Task PublishFault<T>(PublishContext<T> pubContext, Exception exception) where T : class
        {
            return FaultHandleSendEndpoint(pubContext, exception);
        }

        public Task SendFault<T>(SendContext<T> sendContext, Exception exception) where T : class
        {
            return FaultHandleSendEndpoint(sendContext, exception);
        }

        private Task PreHandleSendEndpoint<T>(SendContext<T> sendContext, string isSendEndpoint = null) where T : class
        {
            _contexts[sendContext.ConversationId.Value] = _entrySegmentContextAccessor.Context;

            var host = $"{sendContext.DestinationAddress.Host}";
            var activity = Activity.Current ?? default;

            var context = _tracingContext.CreateExitSegmentContext((isSendEndpoint ?? OperateNamePrefix) + activity.OperationName,
                host, new MasstransitCarrierHeaderCollection(sendContext.Headers));

            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = _getComponentID.GetPublishComponentID(sendContext);
            context.Span.Peer = host;
            context.Span.AddTag(Tags.MQ_TOPIC, activity.OperationName);
            context.Span.AddTag(Tags.MQ_BROKER, sendContext.DestinationAddress.Host);
            context.Span.AddTag(MassTags.Durable, sendContext.Durable);
            context.Span.AddTag(MassTags.FaultAddress, sendContext.FaultAddress?.AbsolutePath);
            context.Span.AddTag(MassTags.SentTime, sendContext.SentTime.Value.ToString("yyyy-MM-dd hh:mm:ss-fff"));

            context.Span.AddLog(LogEvent.Event("Masstransit Message Publishing Start"));
            context.Span.AddLog(LogEvent.Message("Masstransit message publishing start..."));
            return Task.CompletedTask;
        }

        private Task PostHandleSendEndpoint<T>(SendContext<T> sendContect) where T : class
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;
            foreach (var tags in activity.Tags)
            {
                context.Span.AddTag(tags.Key, tags.Value);
            }
            context.Span.AddLog(LogEvent.Event("Masstransit Message Publishing End"));
            context.Span.AddLog(LogEvent.Message($"Masstransit message published successfully!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration.TotalMilliseconds }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { sendContect.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" +
                                                 $"--> Message Type: {sendContect.Message.GetType()} {Environment.NewLine}" +
                                                 $"--> Message Json: {JsonSerializer.Serialize(sendContect.Message)}"));
            _tracingContext.Release(context);
            _contexts.TryRemove(sendContect.ConversationId.Value, out _);
            return Task.CompletedTask;
        }

        private Task FaultHandleSendEndpoint<T>(SendContext<T> sendContect, Exception exception) where T : class
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;

            foreach (var tags in activity.Tags)
            {
                context.Span.AddTag(tags.Key, tags.Value);
            }
            context.Span.AddLog(LogEvent.Event("Masstransit Message Publishing Error"));
            context.Span.AddLog(LogEvent.Message($"Masstransit message publishing failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { sendContect.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" +
                                                 $"--> Message Type: {sendContect.Message.GetType()} {Environment.NewLine}" +
                                                 $"--> Message Json: {JsonSerializer.Serialize(sendContect.Message)}"));
            context.Span.ErrorOccurred(exception, _tracingConfig);

            _tracingContext.Release(context);
            _contexts.TryRemove(sendContect.ConversationId.Value, out _);
            return Task.CompletedTask;
        }
    }
}
