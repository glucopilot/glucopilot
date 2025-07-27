using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Pens;

[ExcludeFromCodeCoverage]
public static class PensEndpoints
{
    internal static IEndpointRouteBuilder MapPensEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/pens")
            .WithTags("Pens");

        group.MapGet("/", ListPens.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("ListPens")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/", NewPen.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("CreatePen")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemovePen.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("DeletePen")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
