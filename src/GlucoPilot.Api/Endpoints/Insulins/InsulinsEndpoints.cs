using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
namespace GlucoPilot.Api.Endpoints.Insulins;

[ExcludeFromCodeCoverage]
public static class InsulinsEndpoints
{
    internal static IEndpointRouteBuilder MapInsulinsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/insulins")
            .WithTags("Insulins");

        group.MapGet("/", List.Endpoint.HandleAsync)
            .WithName("ListInsulins")
            .HasApiVersion(1.0)
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetInsulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("GetInsulin")
            .RequireAuthorization();

        group.MapPost("/", NewInsulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("CreateInsulin")
            .RequireAuthorization();

        group.MapPatch("/{id:guid}", UpdateInsulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("UpdateInsulin")
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemoveInsulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("DeleteInsulin")
            .RequireAuthorization();

        return endpoints;
    }
}
