using DotNetCore.CAP.Diagnostics;
using SkyApm.Config;
using System;
using CapEvents = DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames;

namespace SkyApm.Diagnostics.CAP
{
    public class CapTracingDiagnosticProcessorAdapter : ICapDiagnosticProcessor
    {
        private readonly ICapDiagnosticProcessor _processor;

        public CapTracingDiagnosticProcessorAdapter(
            IConfigAccessor configAccessor,
            CapTracingDiagnosticProcessor defaultProcessor,
            SpanCapTracingDiagnosticProcessor spanProcessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (ICapDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => CapEvents.DiagnosticListenerName;

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            _processor.BeforePublishStore(eventData);
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            _processor.AfterPublishStore(eventData);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubStore eventData)
        {
            _processor.ErrorPublishStore(eventData);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            _processor.BeforePublish(eventData);
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            _processor.AfterPublish(eventData);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            _processor.ErrorPublish(eventData);
        }


        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            _processor.CapBeforeConsume(eventData);
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            _processor.CapAfterConsume(eventData);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            _processor.CapErrorConsume(eventData);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            _processor.CapBeforeSubscriberInvoke(eventData);
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            _processor.CapAfterSubscriberInvoke(eventData);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            _processor.CapErrorSubscriberInvoke(eventData);
        }
    }
}
