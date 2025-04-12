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

        group.MapPost("/", NewInsulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
