using SkyApm.Common;

namespace SkyApm.Diagnostics.MassTransit.Common
{
    public class RabbitmqComponentIdChecker : IComponentIdChecker
    {
        public string PublishEndpointName => "rabbitmq";

        public string ConsumeEndpointName => PublishEndpointName;

        public StringOrIntValue CheckPublishComponentID(string host)
        {
            if (ContainsRabbitmq(host))
                return 52; //rabbitmq-producer 
            return 51; //rabbitmq
        }

        public StringOrIntValue CheckConsumeComponentID(string host)
        {
            if (ContainsRabbitmq(host))
                return 53; //rabbitmq-consumer 
            return 51; //rabbitmq
        }

        private bool ContainsRabbitmq(string host)
        {
            if (host.Contains("rabbitmq"))
                return true; // if url contains rabbitmq
            return false;
        }
    }
}
