using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Ingredients;

[ExcludeFromCodeCoverage]
public static class IngredientsEndpoints
{
    internal static IEndpointRouteBuilder MapIngredientsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/ingredients")
            .WithTags("Ingredients");

        group.MapGet("/", List.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        group.MapPost("/", NewIngredient.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        group.MapPatch("/", UpdateIngredient.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        return endpoints;
    }
}
