using MassTransit;
using SkyApm.Common;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public interface IGetComponentUtil
    {
        StringOrIntValue GetPublishComponentID<T>(T context) where T : SendContext;
        StringOrIntValue GetConsumeComponentID<T>(T contect) where T : ConsumeContext;
    }
}
