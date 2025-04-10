using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

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
            .RequireAuthorization();

        group.MapGet("/login", Login.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        return endpoints;
    }
}
