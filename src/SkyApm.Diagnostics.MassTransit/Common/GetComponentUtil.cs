using MassTransit;
using SkyApm.Common;
using System;
using System.Collections.Generic;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public class GetComponentUtil : IGetComponentUtil
    {
        private readonly IEnumerable<IComponentIdChecker> checkers;

        public GetComponentUtil(IEnumerable<IComponentIdChecker> checkers)
        {
            this.checkers = checkers;
        }
        public StringOrIntValue GetPublishComponentID<T>(T context) where T : SendContext
        {
            var host = context.DestinationAddress.AbsoluteUri;
            foreach (var checker in checkers)
            {
                if (host.Contains(checker.PublishEndpointName, StringComparison.OrdinalIgnoreCase))
                {
                    return checker.CheckPublishComponentID(host);
                }
            }
            return new StringOrIntValue(0, "Unknown");
        }

        public StringOrIntValue GetConsumeComponentID<T>(T context) where T : ConsumeContext
        {
            var host = context.SourceAddress.AbsoluteUri;
            foreach (var checker in checkers)
            {
                if (host.Contains(checker.ConsumeEndpointName, StringComparison.OrdinalIgnoreCase))
                {
                    return checker.CheckConsumeComponentID(host);
                }
            }
            return new StringOrIntValue(0, "Unknown");
        }
    }
}
