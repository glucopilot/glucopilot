using GlucoPilot.Api.Endpoints.Meals;
using GlucoPilot.Api.Endpoints.Readings;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using GlucoPilot.Api.Endpoints.LibreLink;
using GlucoPilot.Api.Endpoints.Ingredients;
using GlucoPilot.Api.Endpoints.Insulins;
using GlucoPilot.Api.Endpoints.Injections;

namespace GlucoPilot.Api.Endpoints;

[ExcludeFromCodeCoverage]
public static class GlucoPilotEndpoints
{
    internal static IEndpointRouteBuilder MapGlucoPilotEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapReadingsEndpoints();
        endpoints.MapMealsEndpoints();
        endpoints.MapLibreLinkEndpoints();
        endpoints.MapIngredientsEndpoints();
        endpoints.MapInsulinsEndpoints();
        endpoints.MapInjectionsEndpoints();

        return endpoints;
    }
}
