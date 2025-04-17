using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Sensors;

[ExcludeFromCodeCoverage]
public static class SensorsEndpoints
{
    internal static IEndpointRouteBuilder MapSensorsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/sensors")
            .WithTags("Sensors");

        group.MapGet("/", List.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemoveSensor.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
