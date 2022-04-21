using DotNetCore.CAP.Diagnostics;

namespace SkyApm.Diagnostics.CAP
{
    public interface ICapDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        void BeforePublishStore(CapEventDataPubStore eventData);

        void AfterPublishStore(CapEventDataPubStore eventData);

        void ErrorPublishStore(CapEventDataPubStore eventData);

        void BeforePublish(CapEventDataPubSend eventData);

        void AfterPublish(CapEventDataPubSend eventData);

        void ErrorPublish(CapEventDataPubSend eventData);

        void CapBeforeConsume(CapEventDataSubStore eventData);

        void CapAfterConsume(CapEventDataSubStore eventData);

        void CapErrorConsume(CapEventDataSubStore eventData);

        void CapBeforeSubscriberInvoke(CapEventDataSubExecute eventData);

        void CapAfterSubscriberInvoke(CapEventDataSubExecute eventData);

        void CapErrorSubscriberInvoke(CapEventDataSubExecute eventData);
    }
}
