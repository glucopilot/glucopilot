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
            .RequireAuthorization();

        group.MapGet("/average-glucose", AverageGlucose.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("AverageGlucoseInsight")
            .RequireAuthorization();

        group.MapGet("/average-nutrition", AverageNutrition.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("AverageNutritionInsight")
            .RequireAuthorization();

        group.MapGet("/insulin-to-carb-ratio", InsulinToCarbRatio.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("InsulinToCarbRatioInsight")
            .RequireAuthorization();

        group.MapGet("/insulin-on-board", InsulinOnBoard.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("InsulinOnBoardInsight")
            .RequireAuthorization();

        group.MapGet("/time-in-range", TimeInRange.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("TimeInRangeInsight")
            .RequireAuthorization();

        group.MapGet("/total-nutrition", TotalNutrition.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("TotalNutritionInsight")
            .RequireAuthorization();

        return endpoints;
    }
}