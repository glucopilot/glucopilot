using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Identity.Endpoints;

[ExcludeFromCodeCoverage]
internal static class IdentityEndpoints
{
    internal static IEndpointRouteBuilder MapIdentityEndpointsInternal(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/identity").WithTags("Identity");

        group.MapPost("/login", Login.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .AllowAnonymous();
        group.MapPost("/register", Register.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .AllowAnonymous();
        group.MapGet("/verify-email", VerifyEmail.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .AllowAnonymous();
        group.MapGet("/is-verified", IsVerified.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .AllowAnonymous();

        return endpoints;
    }
}