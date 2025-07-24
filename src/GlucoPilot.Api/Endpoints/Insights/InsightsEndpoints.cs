using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Api.Endpoints.Insights;

[ExcludeFromCodeCoverage]
internal static class InsightsEndpoints
{
    internal static IEndpointRouteBuilder MapInsightsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/insights")
            .WithTags("Insights");

        group.MapGet("/insulin", Insulin.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("InsulinInsight")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/average-glucose", AverageGlucose.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("AverageGlucoseInsight")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/average-nutrition", AverageNutrition.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("AverageNutritionInsight")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/insulin-to-carb-ratio", InsulinToCarbRatio.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("InsulinToCarbRatioInsight")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}