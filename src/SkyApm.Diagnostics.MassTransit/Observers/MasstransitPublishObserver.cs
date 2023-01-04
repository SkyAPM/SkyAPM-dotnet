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
    public class MasstransitPublishObserver : IPublishObserver
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
            _contexts[pubContext.ConversationId.Value] = _entrySegmentContextAccessor.Context;

            var host = $"{pubContext.DestinationAddress.Host}";
            var activity = Activity.Current ?? default;

            var context = _tracingContext.CreateExitSegmentContext(OperateNamePrefix + activity.OperationName,
                host, new MasstransitCarrierHeaderCollection(pubContext.Headers));

            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = _getComponentID.GetPublishComponentID(pubContext);
            context.Span.Peer = host;
            context.Span.AddTag(Tags.MQ_TOPIC, activity.OperationName);
            context.Span.AddTag(Tags.MQ_BROKER, pubContext.DestinationAddress.Host);
            context.Span.AddTag(MassTags.Durable, pubContext.Durable);
            context.Span.AddTag(MassTags.FaultAddress, pubContext.FaultAddress?.AbsolutePath);
            context.Span.AddTag(MassTags.SentTime, pubContext.SentTime.Value.ToString("yyyy-MM-dd hh:mm:ss-fff")); 

            context.Span.AddLog(LogEvent.Event("Masstransit Message Publishing Start"));
            context.Span.AddLog(LogEvent.Message("Masstransit message publishing start..."));
            return Task.CompletedTask;
        }

        public Task PostPublish<T>(PublishContext<T> pubContext) where T : class
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
                                                 $"--> Message Id: { pubContext.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" + 
                                                 $"--> Message Type: {pubContext.Message.GetType()} {Environment.NewLine}" + 
                                                 $"--> Message Json: {JsonSerializer.Serialize(pubContext.Message)}"));
            _tracingContext.Release(context);
            _contexts.TryRemove(pubContext.ConversationId.Value, out _);
            return Task.CompletedTask;
        }

        public Task PublishFault<T>(PublishContext<T> pubContext, Exception exception) where T : class
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
                                                 $"--> Message Id: { pubContext.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" +
                                                 $"--> Message Type: {pubContext.Message.GetType()} {Environment.NewLine}" +
                                                 $"--> Message Json: {JsonSerializer.Serialize(pubContext.Message)}"));
            context.Span.ErrorOccurred(exception, _tracingConfig);

            _tracingContext.Release(context);
            _contexts.TryRemove(pubContext.ConversationId.Value, out _);
            return Task.CompletedTask;
        }
    }
}
