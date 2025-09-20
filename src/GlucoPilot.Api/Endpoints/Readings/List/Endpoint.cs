using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
            [FromServices] IRepository<Patient> patientRepository,
            CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var patient = await patientRepository.FindOneAsync(u => u.Id == userId,
                new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true }, cancellationToken)
            .ConfigureAwait(false);

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

        return TypedResults.Ok(readings);
    }
}
