using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoPilot.Identity;

[ExcludeFromCodeCoverage]
public static class Startup
{
    /// <summary>
    /// Adds identity services to the service collection.
    /// </summary>
    /// <param name="services">Service collection to add identity services to.</param>
    /// <param name="configure">Configures the <see cref="IdentityOptions"/>.</param>
    /// <returns>Service collection containing identity services.</returns>
    public static IServiceCollection AddIdentity(this IServiceCollection services, Action<IdentityOptions> configure)
    {
        services.AddApiVersioning();
        services.AddProblemDetails();
        services.AddValidatorsFromAssemblyContaining(typeof(Startup));

        services.AddOptions<IdentityOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        
        return services
            .AddIdentityAuthentication(configure);
    }

    /// <summary>
    /// Registers identity middleware in the application pipeline.
    /// </summary>
    /// <param name="app">Application builder to register identity middleware to.</param>
    /// <returns>Application builder with registered identity middlewares.</returns>
    public static IApplicationBuilder UseIdentity(this IApplicationBuilder app)
    {
        return app
            .UseIdentityAuthentication();
    }   
}