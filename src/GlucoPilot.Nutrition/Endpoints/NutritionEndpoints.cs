using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Nutrition.Endpoints.GetProduct;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Nutrition.Endpoints;

[ExcludeFromCodeCoverage]
internal static class NutritionEndpoints
{
    internal static IEndpointRouteBuilder MapNutritionEndpointsInternal(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/nutrition").WithTags("Nutrition");

        group.MapGet("/products/{code}", GetProduct.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ProductResponse>()
            .RequireAuthorization();
        
        return endpoints;
    }
}