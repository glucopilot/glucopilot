using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;

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
            .WithName("ListIngredients")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
        
        group.MapPost("/", NewIngredient.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("CreateIngredient")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPatch("/{id:guid}", UpdateIngredient.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("UpdateIngredient")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemoveIngredient.Endpoint.HandleAsync)
            .WithName("DeleteIngredient")
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
