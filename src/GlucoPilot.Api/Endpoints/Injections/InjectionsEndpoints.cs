using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Injections;

[ExcludeFromCodeCoverage]
public static class InjectionsEndpoints
{
    internal static IEndpointRouteBuilder MapInjectionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/injections")
            .WithTags("Injections");

        group.MapGet("/{id:guid}", GetInjection.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("GetInjection")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/", ListInjections.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("ListInjections")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/", NewInjection.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("NewInjection")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPatch("/{id:guid}", UpdateInjection.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("UpdateInjection")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemoveInjection.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("DeleteInjection")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
