using Microsoft.Extensions.DependencyInjection;

namespace SkyWalking.Diagnostics.EntityFrameworkCore
{
    public static class DatabaseProviderBuilderExtensions
    {
        public static DatabaseProviderBuilder AddPomeloMysql(this DatabaseProviderBuilder builder)
        {
            builder.Services.AddSingleton<IEfCoreSpanMetadataProvider, MySqlEFCoreSpanMetadataProvider>();
            return builder;
        }
    }
}