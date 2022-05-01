using SkyApm.Utilities.StaticAccessor;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSkyAPMStaticAccessor(this IServiceCollection services)
        {
            services.AddHostedService<StaticAccessorHostedService>();

            return services;
        }
    }
}
