using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GlucoPilot.Identity.Authentication;

[ExcludeFromCodeCoverage]
internal static class Startup
{
    internal static IServiceCollection AddIdentityAuthentication(this IServiceCollection services,
        Action<IdentityOptions> configure)
    {
        var identityOptions = new IdentityOptions();
        configure?.Invoke(identityOptions);
        var key = Encoding.ASCII.GetBytes(identityOptions.TokenSigningKey);

        services
            .AddScoped<CurrentUserMiddleware>()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped<ICurrentUserInitializer>(provider =>
                (ICurrentUserInitializer)provider.GetRequiredService<ICurrentUser>());
        services
            .AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
#if DEBUG
                bearer.RequireHttpsMetadata = false;
#else
                bearer.RequireHttpsMetadata = true;
#endif
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            throw new UnauthorizedException("NOT_LOGGED_IN");
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = _ => throw new ForbiddenException("NOT_AUTHORIZED"),
                };
            });
        services.AddAuthorizationBuilder();

        return services;
    }

    internal static IApplicationBuilder UseIdentityAuthentication(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseAuthorization()
            .UseMiddleware<CurrentUserMiddleware>();
    }
}