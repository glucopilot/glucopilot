using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GlucoPilot.Data
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDatabaseHealthChecks(this IHealthChecksBuilder builder)
        {
            return builder.AddDbContextCheck<GlucoPilotDbContext>("GlucoPilot-Database");
        }
    }
}