using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insights.TimeInRange;

internal static class Endpoint
{
    private const double HighGlucoseThresholdOffset = 3d;
    private const double MaxGlucoseLevel = double.MaxValue;

    internal static async Task<Results<Ok<TimeInRangeResponse>, ValidationProblem, UnauthorizedHttpResult>>
        HandleAsync(
            [AsParameters] TimeInRangeRequest request,
            [FromServices] IValidator<TimeInRangeRequest> validator,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<GlucoseRange> rangeRepository,
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

        var sqlQuery = @"
        WITH UserTargets AS (
            SELECT LowSugarThreshold, HighSugarThreshold
            FROM user_settings
            WHERE UserId = {0}
        ),
        TimeRanges AS (
            SELECT 
                r.Created AS startTime,
                LEAD(r.Created) OVER (ORDER BY r.Created) AS endTime,
                r.GlucoseLevel,
                ut.LowSugarThreshold,
                ut.HighSugarThreshold
            FROM readings r
            CROSS JOIN UserTargets ut
            WHERE r.UserId = {0} AND r.Created BETWEEN {1} AND {2}
        ),
        TimeInRange AS (
            SELECT 
                CASE 
                    WHEN GlucoseLevel BETWEEN 0 AND LowSugarThreshold THEN 0
                    WHEN GlucoseLevel BETWEEN HighSugarThreshold AND HighSugarThreshold + 3 THEN 2
                    When GlucoseLevel > HighSugarThreshold + {3} THEN 3
                    ELSE 1
                END AS RangeID,
                DATEDIFF(minute, startTime, endTime) AS Duration
            FROM TimeRanges
            WHERE endTime IS NOT NULL
        )
        SELECT 
            RangeID,
            SUM(Duration) AS TotalMinutes,
            (SUM(Duration) * 100.0 / (SELECT SUM(Duration) FROM TimeInRange)) AS Percentage,
            MIN(CASE 
                WHEN RangeID = 0 THEN 0
                WHEN RangeID = 1 THEN LowSugarThreshold
                WHEN RangeID = 2 THEN HighSugarThreshold
                WHEN RangeID = 3 THEN HighSugarThreshold + {3}
                ELSE NULL
            END) AS RangeMin,
            MAX(CASE 
                WHEN RangeID = 0 THEN LowSugarThreshold
                WHEN RangeID = 1 THEN HighSugarThreshold
                WHEN RangeID = 2 THEN HighSugarThreshold + {3}
                WHEN RangeID = 3 THEN {4}
                ELSE NULL
            END) AS RangeMax
        FROM TimeInRange
        CROSS JOIN (SELECT LowSugarThreshold, HighSugarThreshold FROM UserTargets) ut
        GROUP BY RangeID, LowSugarThreshold, HighSugarThreshold
        ORDER BY RangeID;";

        var results = rangeRepository.FromSqlRaw<GlucoseRange>(sqlQuery, new FindOptions { IsAsNoTracking = true }, userId, from, to, HighGlucoseThresholdOffset, MaxGlucoseLevel)
            .AsEnumerable()
            .ToList();

        var response = new TimeInRangeResponse
        {
            To = to,
            From = from,
            Ranges = results
        };

        return TypedResults.Ok(response);
    }
}
