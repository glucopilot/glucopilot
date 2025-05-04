using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Api.Endpoints.Insights.AverageGlucose;

internal static class Endpoint
{
    internal static async Task<Results<Ok<double>, ValidationProblem, UnauthorizedHttpResult>> HandleAsync(
        [AsParameters] AverageGlucoseRequest request,
        [FromServices] IValidator<AverageGlucoseRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Reading> repository,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();

        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddDays(-7);
        
        var validationResult = await validator .ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
        
        var average = repository
            .GetAll(new FindOptions { IsAsNoTracking = true })
            .Where(r => r.UserId == userId && r.Created >= from && r.Created <= to)
            .DefaultIfEmpty()
            .Average(r => r != null ? r.GlucoseLevel : 0);

        return TypedResults.Ok(average);
    }
}