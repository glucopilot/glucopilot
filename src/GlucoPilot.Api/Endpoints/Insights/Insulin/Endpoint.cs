using System.Linq;
using System.Threading;
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
    internal static Results<Ok<InsulinInsightsResponse>, UnauthorizedHttpResult> Handle(
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> repository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var treatments = repository.Find(t => t.UserId == userId && t.Injection != null && t.Injection.Insulin != null,
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