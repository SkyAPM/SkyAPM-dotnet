using System;

namespace SkyWalking
{
    public class RuntimeEnvironment : IRuntimeEnvironment
    {
        public NullableValue ApplicationId { get; internal set; }

        public NullableValue ApplicationInstanceId { get; internal set; }

        public bool Initialized => ApplicationId.HasValue && ApplicationInstanceId.HasValue;
        
        public Guid AgentUUID { get; } = Guid.NewGuid();
    }
}