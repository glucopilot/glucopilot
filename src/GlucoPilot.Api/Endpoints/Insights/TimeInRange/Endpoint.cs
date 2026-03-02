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
        ),
        AllRanges AS (
            SELECT 0 AS RangeID
            UNION ALL SELECT 1
            UNION ALL SELECT 2
            UNION ALL SELECT 3
        ),
        TotalDuration AS (
            SELECT COALESCE(SUM(Duration), 0) AS Total FROM TimeInRange
        )
        SELECT 
            ar.RangeID,
            COALESCE(SUM(tir.Duration), 0) AS TotalMinutes,
            CASE WHEN td.Total > 0 THEN (COALESCE(SUM(tir.Duration), 0) * 100.0 / td.Total) ELSE 0 END AS Percentage,
            CASE 
                WHEN ar.RangeID = 0 THEN 0
                WHEN ar.RangeID = 1 THEN ut.LowSugarThreshold
                WHEN ar.RangeID = 2 THEN ut.HighSugarThreshold
                WHEN ar.RangeID = 3 THEN ut.HighSugarThreshold + {3}
            END AS RangeMin,
            CASE 
                WHEN ar.RangeID = 0 THEN ut.LowSugarThreshold
                WHEN ar.RangeID = 1 THEN ut.HighSugarThreshold
                WHEN ar.RangeID = 2 THEN ut.HighSugarThreshold + {3}
                WHEN ar.RangeID = 3 THEN {4}
            END AS RangeMax
        FROM AllRanges ar
        CROSS JOIN UserTargets ut
        CROSS JOIN TotalDuration td
        LEFT JOIN TimeInRange tir ON ar.RangeID = tir.RangeID
        GROUP BY ar.RangeID, ut.LowSugarThreshold, ut.HighSugarThreshold, td.Total
        ORDER BY ar.RangeID;";

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
