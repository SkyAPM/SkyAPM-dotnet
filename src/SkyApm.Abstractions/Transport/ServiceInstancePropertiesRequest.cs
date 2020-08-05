using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Transport
{
    public class ServiceInstancePropertiesRequest
    {
        public string ServiceName { get; set; }

        public string InstanceUUID { get; set; }

        public AgentOsInfoRequest Properties { get; set; }
    }
}
