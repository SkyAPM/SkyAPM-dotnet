namespace SkyApm.Transport; 

public class ServiceInstancePropertiesRequest
{
    public string ServiceId { get; set; }

    public string ServiceInstanceId { get; set; }

    public AgentOsInfoRequest Properties { get; set; }
}