using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace GlucoPilot.Data
{
    public static class Startup
    {
        public static IServiceCollection AddData(this IServiceCollection services, Action<DatabaseOptions> configure)
        {
            return services
                .Configure(configure)
                .AddScoped<GlucoPilotDbInitialiser>()
                .AddDbContext<GlucoPilotDbContext>((provider, options) =>
                {
                    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    options.UseSqlServer(databaseOptions.ConnectionString, e => e.MigrationsAssembly("TapTapGlucose.Data.Migrations"));

#if DEBUG
                    options.ConfigureWarnings(
                    warnings => warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
#endif
                });
        }
    }
}
