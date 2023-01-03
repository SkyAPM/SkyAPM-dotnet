using SkyApm.Common;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public interface IComponentIdChecker
    {
        string PublishEndpointName { get; }
        string ConsumeEndpointName { get; }
        StringOrIntValue CheckPublishComponentID(string host);
        StringOrIntValue CheckConsumeComponentID(string host);
    }
}
