namespace SkyWalking
{
    public class RuntimeEnvironment : IRuntimeEnvironment
    {
        public NullableValue ApplicationId { get; internal set; }

        public NullableValue ApplicationInstanceId { get; internal set; }

        public string ApplicationCode { get; internal set; }

        public bool Initialized => ApplicationId.HasValue && ApplicationInstanceId.HasValue && !string.IsNullOrWhiteSpace(ApplicationCode);
    }
}