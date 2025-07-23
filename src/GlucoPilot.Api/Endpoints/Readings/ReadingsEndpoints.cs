using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;

namespace GlucoPilot.Api.Endpoints.Readings;

[ExcludeFromCodeCoverage]
public static class ReadingsEndpoints
{
    internal static IEndpointRouteBuilder MapReadingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/readings")
            .WithTags("Readings");

        group.MapGet("/", List.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("ListReadings")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/", NewReading.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("NewReading")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
