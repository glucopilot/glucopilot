using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using ReadingDirection = GlucoPilot.Api.Models.ReadingDirection;

namespace GlucoPilot.Api.Endpoints.Readings.List;

internal static class Endpoint
{
    internal static async Task<Results<Ok<List<ReadingsResponse>>, UnauthorizedHttpResult, ValidationProblem>>
        HandleAsync(
            [AsParameters] ListReadingsRequest request,
            [FromServices] IValidator<ListReadingsRequest> validator,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<Reading> repository,
            CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        if (request.To is null)
        {
            request.To = DateTime.UtcNow;
        }

        var query = """
                                    WITH QuarterHourIntervals AS (
                        SELECT 
                            CONVERT(
                                TIME,
                                DATEADD(MINUTE, (DATEPART(MINUTE, [Created]) / {0}) * {0}, 
                                DATEADD(HOUR, DATEPART(HOUR, [Created]), 0))
                            ) AS quarter_hour_start,
                            [Id],
                            [UserId],
                            [Created],
                            [GlucoseLevel],
                            [Direction],
                            ROW_NUMBER() OVER (
                                PARTITION BY 
                                    DATEADD(MINUTE, (DATEPART(MINUTE, [Created]) / {0}) * {0}, 
                                    DATEADD(HOUR, DATEPART(HOUR, [Created]), 0)) 
                                ORDER BY [Created] DESC
                            ) AS row_num
                        FROM 
                            [readings]
                    )

                    SELECT 
                        [Id],
                        [UserId],
                        [Created],
                        [GlucoseLevel],
                        [Direction]
                    FROM 
                        QuarterHourIntervals
                    WHERE 
                        row_num = 1
                        AND [Created] BETWEEN {1} AND {2}
                            AND [UserId] = {3}
                    ORDER BY 
                        [Created] DESC;
                    """;

        var readings = repository.FromSqlRaw(query, new FindOptions { IsAsNoTracking = true }, request.MinuteInterval,
                request.From, request.To,
                userId).AsEnumerable()
            .Select(r => new ReadingsResponse
            {
                UserId = r.UserId,
                Id = r.Id,
                Created = r.Created,
                GlucoseLevel = r.GlucoseLevel,
                Direction = (ReadingDirection)r.Direction
            })
            .ToList();

        if (readings.Count > 0)
        {
            var first = readings.First();
            if (request.To.Value - first.Created > TimeSpan.FromMinutes(1))
            {
                var additionalReadings = repository.Find(
                    r => r.UserId == userId &&
                         r.Created > first.Created &&
                         r.Created <= request.To.Value,
                    new FindOptions { IsAsNoTracking = true })
                    .OrderBy(r => r.Created)
                    .Select(r => new ReadingsResponse
                    {
                        UserId = r.UserId,
                        Id = r.Id,
                        Created = r.Created,
                        GlucoseLevel = r.GlucoseLevel,
                        Direction = (ReadingDirection)r.Direction
                    })
                    .ToList();

                if (additionalReadings.Count > 0)
                {
                    readings.Insert(0, additionalReadings.First());
                }
            }

            var last = readings.Last();
            if (last.Created - request.From > TimeSpan.FromMinutes(1))
            {
                var additionalReadings = repository.Find(
                    r => r.UserId == userId &&
                         r.Created >= request.From &&
                         r.Created < last.Created,
                    new FindOptions { IsAsNoTracking = true })
                    .OrderByDescending(r => r.Created)
                    .Select(r => new ReadingsResponse
                    {
                        UserId = r.UserId,
                        Id = r.Id,
                        Created = r.Created,
                        GlucoseLevel = r.GlucoseLevel,
                        Direction = (ReadingDirection)r.Direction
                    })
                    .ToList();

                if (additionalReadings.Count > 0)
                {
                    readings.Add(additionalReadings.Last());
                }
            }
        }

        return TypedResults.Ok(readings);
    }
}
