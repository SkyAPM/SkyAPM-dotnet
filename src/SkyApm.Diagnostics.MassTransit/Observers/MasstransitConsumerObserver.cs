using MassTransit;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.MassTransit.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    public class MasstransitConsumerObserver : IConsumeObserver
    {
        private const string OperateNamePrefix = "Masstransit Consumer/";

        private readonly ITracingContext _tracingContext;
        private readonly IGetComponentUtil _getComponentID;
        private TracingConfig _tracingConfig;
        public MasstransitConsumerObserver(ITracingContext tracingContext,
            IGetComponentUtil getComponentID,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _getComponentID = getComponentID;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }
        public Task PreConsume<T>(ConsumeContext<T> consumeContext) where T : class
        {
            var activity = Activity.Current ?? default;

            var operationName = OperateNamePrefix + activity.OperationName;
            var context = _tracingContext.CreateLocalSegmentContext(operationName);
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = _getComponentID.GetConsumeComponentID(consumeContext);
            context.Span.Peer = $"{consumeContext.DestinationAddress.Host}";
            context.Span.AddTag(Tags.MQ_TOPIC, activity.OperationName);
            context.Span.AddTag(Tags.MQ_BROKER, consumeContext.DestinationAddress.Host);
            context.Span.AddTag(MassTags.ExpirationTime, consumeContext.ExpirationTime?.ToString("yyyy-MM-dd HH:mm:ss-fff"));
            context.Span.AddLog(LogEvent.Event("Masstransit Message Consumed Start"));
            context.Span.AddLog(LogEvent.Message("Masstransit message consumed start..."));

            ObserverSegmentContextDictionary.Contexts[consumeContext.MessageId.Value] = context;
            return Task.CompletedTask;
        }
        public Task PostConsume<T>(ConsumeContext<T> consumeContext) where T : class
        {
            var context = ObserverSegmentContextDictionary.Contexts[consumeContext.MessageId.Value];
            if (context == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;

            context.Span.AddLog(LogEvent.Event("Masstransit Message Consumed End"));
            context.Span.AddLog(LogEvent.Message($"Masstransit message consumed succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration.TotalMilliseconds }ms. {Environment.NewLine}" +
                                                 $"--> Message Id: { consumeContext.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" +
                                                 $"--> Message Type: {consumeContext.Message.GetType()} {Environment.NewLine}" +
                                                 $"--> Message Json: {JsonSerializer.Serialize(consumeContext.Message)}"));

            _tracingContext.Release(context);
            ObserverSegmentContextDictionary.Contexts.TryRemove(consumeContext.MessageId.Value, out _);
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> consumeContext, Exception exception) where T : class
        {
            var context = ObserverSegmentContextDictionary.Contexts[consumeContext.MessageId.Value];
            if (context == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;

            context.Span.AddLog(LogEvent.Event("Masstransit Message Consumed Error"));
            context.Span.AddLog(LogEvent.Message($"Masstransit message consumed failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration.TotalMilliseconds }ms. {Environment.NewLine}" +
                                                 $"--> Message Id: { consumeContext.MessageId }, Name: {activity.OperationName} {Environment.NewLine}" +
                                                 $"--> Message Type: {consumeContext.Message.GetType()} {Environment.NewLine}" +
                                                 $"--> Message Json: {JsonSerializer.Serialize(consumeContext.Message)} "));
            context.Span.ErrorOccurred(exception, _tracingConfig);
            _tracingContext.Release(context);
            ObserverSegmentContextDictionary.Contexts.TryRemove(consumeContext.MessageId.Value, out _);
            return Task.CompletedTask;
        }
    }
}
