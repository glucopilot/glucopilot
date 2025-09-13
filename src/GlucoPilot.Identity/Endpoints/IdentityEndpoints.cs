using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            .Produces<ErrorResult>(StatusCodes.Status409Conflict)
            .AllowAnonymous();
        group.MapGet("/verify-email", VerifyEmail.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ContentResult>()
            .AllowAnonymous();
        group.MapPost("/send-verification", SendVerification.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .AllowAnonymous();
        group.MapPost("/refresh-token", RefreshToken.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
        group.MapPost("/revoke-token", RevokeToken.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}