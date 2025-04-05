using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.Data;
using GlucoPilot.Data.Entities;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Readings;

internal static class List
{
    internal static async Task<Results<Ok<List<ReadingsResponse>>, ValidationProblem>> HandleAsync(
        [FromQuery] ListReadingsRequest request,
        [FromServices] IValidator<ListReadingsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] GlucoPilotDbContext db,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var readings = await db.Readings
            .Where(r => r.UserId == currentUser.GetUserId())
            .ToListAsync(cancellationToken);

        var filteredReadings = readings
            .Where(r => r.Created >= request.from && r.Created <= request.to)
            .OrderByDescending(r => r.Created.UtcDateTime)
            .ToList();

        var response = filteredReadings.Select(r => new ReadingsResponse
        {
            UserId = r.UserId,
            Id = r.Id,
            Created = r.Created,
            GlucoseLevel = r.GlucoseLevel,
            Direction = r.Direction
        }).ToList();

        return TypedResults.Ok(response);
    }
}
