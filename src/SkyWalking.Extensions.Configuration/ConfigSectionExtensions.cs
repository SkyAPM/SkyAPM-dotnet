using System.Linq;
using SkyWalking.Config;

namespace SkyWalking.Extensions.Configuration
{
    public static class ConfigSectionExtensions
    {
        public static string GetSections(this ConfigAttribute config)
        {
            if (config.Sections == null || config.Sections.Length == 0)
            {
                return null;
            }

            return config.Sections.Length == 1 ? config.Sections[0] : config.Sections.Aggregate((x, y) => x + ":" + y);
        }
    }
}