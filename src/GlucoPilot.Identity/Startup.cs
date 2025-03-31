using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoPilot.Identity
{
    public static class Startup
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning();
            services.AddProblemDetails();

            return services;
        }
        
    }
}
