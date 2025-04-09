using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Meals;

[ExcludeFromCodeCoverage]
public static class MealsEndpoints
{
    internal static IEndpointRouteBuilder MapMealsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/meals")
            .WithTags("Meals");

        group.MapGet("/", List.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        group.MapGet("/{mealId:guid}", GetMeal.HandleAsync)
            .HasApiVersion(1.0)
            .RequireAuthorization();

        return endpoints;
    }
}
