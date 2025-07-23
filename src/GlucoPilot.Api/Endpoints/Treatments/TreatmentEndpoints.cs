using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Treatments;

[ExcludeFromCodeCoverage]
public static class TreatmentsEndpoints
{
    internal static IEndpointRouteBuilder MapTreatmentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/treatments")
            .WithTags("Treatments");

        group.MapGet("/{id:guid}", GetTreatment.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("GetTreatment")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/", NewTreatment.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("NewTreatment")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPatch("/{id:guid}", UpdateTreatment.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("UpdateTreatment")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", RemoveTreatment.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("DeleteTreatment")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/", ListTreatments.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("ListTreatments")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/nutrition", Nutrition.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("GetTreatmentNutrition")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}
