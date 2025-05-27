using GlucoPilot.Api.Endpoints.Meals;
using GlucoPilot.Api.Endpoints.Readings;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using GlucoPilot.Api.Endpoints.LibreLink;
using GlucoPilot.Api.Endpoints.Ingredients;
using GlucoPilot.Api.Endpoints.Insulins;
using GlucoPilot.Api.Endpoints.Injections;
using GlucoPilot.Api.Endpoints.Insights;
using GlucoPilot.Api.Endpoints.Treatments;
using GlucoPilot.Api.Endpoints.Sensors;
using GlucoPilot.Api.Endpoints.Settings;
using GlucoPilot.Api.Endpoints.Pens;

namespace GlucoPilot.Api.Endpoints;

[ExcludeFromCodeCoverage]
public static class GlucoPilotEndpoints
{
    internal static IEndpointRouteBuilder MapGlucoPilotEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapSettingsEndpoints();
        endpoints.MapReadingsEndpoints();
        endpoints.MapMealsEndpoints();
        endpoints.MapLibreLinkEndpoints();
        endpoints.MapIngredientsEndpoints();
        endpoints.MapInsightsEndpoints();
        endpoints.MapInsulinsEndpoints();
        endpoints.MapInjectionsEndpoints();
        endpoints.MapTreatmentsEndpoints();
        endpoints.MapSensorsEndpoints();
        endpoints.MapPensEndpoints();

        return endpoints;
    }
}
