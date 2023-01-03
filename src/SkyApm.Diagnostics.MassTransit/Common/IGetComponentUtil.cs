using MassTransit;
using SkyApm.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public interface IGetComponentUtil
    {
        StringOrIntValue GetPublishComponentID<T>(T context) where T : PublishContext;
        StringOrIntValue GetConsumeComponentID<T>(T contect) where T : ConsumeContext;
    }
}
