using Microsoft.Extensions.DependencyInjection;

namespace SkyWalking.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static SkyWalkingExtensions AddSkyWalkingExtensions(this IServiceCollection services)
        {
            return new SkyWalkingExtensions(services);
        }
    }
}