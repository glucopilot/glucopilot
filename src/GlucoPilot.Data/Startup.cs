using Microsoft.Extensions.DependencyInjection;
using System;

namespace GlucoPilot.Data
{
    public static class Startup
    {
        public static IServiceCollection AddData(this IServiceCollection services, Action<DatabaseOptions> configure)
        {
            return services;
        }
    }
}
