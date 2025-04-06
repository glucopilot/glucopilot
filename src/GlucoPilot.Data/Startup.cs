using GlucoPilot.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Data;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static IServiceCollection AddData(this IServiceCollection services, Action<DatabaseOptions> configure)
    {
        services.AddOptions<DatabaseOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDbContext<GlucoPilotDbContext>((provider, options) =>
            {
                var dbOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.UseDatabase(dbOptions.DbProvider, dbOptions.ConnectionString);
#if DEBUG
                options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
#endif
            });
    }

    public static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider,
        string connectionString)
    {
        switch (dbProvider.ToLowerInvariant())
        {
            case DatabaseOptions.DatabaseProviderKeys.SqlServer:
                return builder.UseSqlServer(connectionString,
                    e => e.MigrationsAssembly("GlucoPilot.Data.Migrators.MSSQL"));
            default:
                throw new InvalidOperationException($"DB provider '{dbProvider}' is not supported.");
        }
    }
}