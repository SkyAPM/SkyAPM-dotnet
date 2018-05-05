using Microsoft.Extensions.DependencyInjection;

namespace SkyWalking.Diagnostics.EntityFrameworkCore
{
    public static class DatabaseProviderBuilderExtensions
    {
        public static DatabaseProviderBuilder AddSqlite(this DatabaseProviderBuilder builder)
        {
            builder.Services.AddSingleton<IEfCoreSpanMetadataProvider, SqliteEFCoreSpanMetadataProvider>();
            return builder;
        }
    }
}
