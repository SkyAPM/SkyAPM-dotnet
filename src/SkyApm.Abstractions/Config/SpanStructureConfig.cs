namespace SkyApm.Config
{
    [Config("SkyWalking", "SpanStructure")]
    public class SpanStructureConfig
    {
        public int MergeDelay { get; set; } = 5000;

        public int MergeQueueSize { get; set; } = 50000;
    }
}
