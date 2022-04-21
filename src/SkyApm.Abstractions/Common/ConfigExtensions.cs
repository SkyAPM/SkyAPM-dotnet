using System;

namespace SkyApm.Config
{
    public static class ConfigExtensions
    {
        public static bool IsSpanStructure(this InstrumentConfig config)
        {
            return "span".Equals(config.StructType, StringComparison.OrdinalIgnoreCase);
        }
    }
}
