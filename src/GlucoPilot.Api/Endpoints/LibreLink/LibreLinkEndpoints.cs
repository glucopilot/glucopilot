using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;

namespace GlucoPilot.Api.Endpoints.LibreLink;

[ExcludeFromCodeCoverage]
public static class LibreLinkEndpoints
{
    internal static IEndpointRouteBuilder MapLibreLinkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/librelink")
            .WithTags("LibreLink");

        group.MapGet("/connections", Connections.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/login", Login.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/pair", PairConnection.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
