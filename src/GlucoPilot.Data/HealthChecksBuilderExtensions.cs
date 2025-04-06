using Microsoft.Extensions.DependencyInjection;

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