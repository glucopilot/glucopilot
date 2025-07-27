using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insights.InsulinOnBoard;

internal static class Endpoint
{
    internal static async Task<Results<Ok<InsulinOnBoardResponse>, UnauthorizedHttpResult>> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentService,
        [FromServices] IRepository<Data.Entities.Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var to = DateTimeOffset.UtcNow;
        var from = to.AddHours(-24);

        var treatments = treatmentService.Find(t => t.UserId == userId &&
                t.Injection != null &&
                t.Injection.Insulin != null &&
                t.Injection.Insulin.Duration.HasValue
                && t.Injection.Created >= from && t.Injection.Created <= to)
            .Include(t => t.Injection)
            .ThenInclude(i => i.Insulin)
            .ToList()
            .Where(t => (DateTimeOffset.UtcNow - t.Injection.Created).TotalHours <= t.Injection.Insulin.Duration.Value)
            .ToList();

        var response = new InsulinOnBoardResponse
        {
            Treatments = treatments.Select(treatment => new InsulinOnBoardTreatmentResponse
            {
                Id = treatment.Id,
                Created = treatment.Created,
                Injection = new InsulinOnBoardInjectionResponse
                {
                    Id = treatment.Injection.Id,
                    Created = treatment.Injection.Created,
                    Units = treatment.Injection.Units,
                    Insulin = new InsulinOnBoardInsulinResponse
                    {
                        Id = treatment.Injection.Insulin.Id,
                        Duration = treatment.Injection.Insulin.Duration ?? 0,
                        PeakTime = treatment.Injection.Insulin.PeakTime ?? 0,
                        Scale = treatment.Injection.Insulin.Scale ?? 0
                    }
                }
            }).ToList(),
        };

        return TypedResults.Ok(response);
    }

    //private static decimal CalculateInsulinOnBoard(Treatment treatment)
    //{
    //    var timeSinceInjection = (DateTimeOffset.UtcNow - treatment.Injection.Created).TotalHours;
    //    var units = treatment.Injection.Units;
    //    var duration = treatment.Injection.Insulin.Duration ?? 0;
    //    var peakTime = treatment.Injection.Insulin.PeakTime ?? 0;
    //    var scale = treatment.Injection.Insulin.Scale ?? 0;

    //    double t = timeSinceInjection / duration;

    //    double activity = (Math.Pow(t, scale) * Math.Exp(-t / peakTime)) / duration;

    //    double iobFraction = 1 - activity;

    //    return (decimal)(units * iobFraction);
    //}
}
