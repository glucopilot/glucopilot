using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Api.Endpoints.Insights.Insulin;

internal static class Endpoint
{
    internal static async Task<Results<Ok<InsulinInsightsResponse>, ValidationProblem, UnauthorizedHttpResult>>
        HandleAsync(
            [AsParameters] InsulinInsightsRequest request,
            [FromServices] IValidator<InsulinInsightsRequest> validator,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<Treatment> repository,
            CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? new DateTimeOffset(to.Date, to.Offset);

        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var treatments = repository.Find(
            t => t.UserId == userId && t.Injection != null && t.Injection.Insulin != null && t.Created >= from &&
                 t.Created <= to,
            new FindOptions { IsAsNoTracking = true }).Include(t => t.Injection).ThenInclude(i => i!.Insulin);
        var insulinInsights = treatments
            .GroupBy(t => t.Injection!.Insulin!.Type)
            .Select(t => new { t.Key, Units = t.Sum(x => x.Injection!.Units) })
            .ToList();

        var result = new InsulinInsightsResponse
        {
            TotalBasalUnits = insulinInsights.FirstOrDefault(x => x.Key == InsulinType.Basal)?.Units ?? 0,
            TotalBolusUnits = insulinInsights.FirstOrDefault(x => x.Key == InsulinType.Bolus)?.Units ?? 0,
        };

        return TypedResults.Ok(result);
    }
}