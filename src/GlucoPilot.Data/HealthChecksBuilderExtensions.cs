using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoPilot.Data
{
    [ExcludeFromCodeCoverage]
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDatabaseHealthChecks(this IHealthChecksBuilder builder)
        {
            return builder.AddDbContextCheck<GlucoPilotDbContext>("GlucoPilot-Database");
        }
    }
}