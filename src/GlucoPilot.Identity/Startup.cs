using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
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
            
        services.AddOptionsWithValidateOnStart<IdentityOptions>()
            .ValidateOnStart();

        return services;
    }

}