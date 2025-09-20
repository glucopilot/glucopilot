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

namespace GlucoPilot.Api.Endpoints.Readings.ListAll;

public class Endpoint
{
    internal static async Task<Results<Ok<List<AllReadingsResponse>>, UnauthorizedHttpResult, ValidationProblem>>
        HandleAsync(
            [AsParameters] ListAllReadingsRequest request,
            [FromServices] IValidator<ListAllReadingsRequest> validator,
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

        var rawReadings = repository.Find(
                r => r.UserId == userId && r.Created >= request.From && r.Created <= request.To,
                new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true })
            .OrderByDescending(r => r.Created)
            .Select(r => new AllReadingsResponse
            {
                UserId = r.UserId,
                Id = r.Id,
                Created = r.Created,
                GlucoseLevel = r.GlucoseLevel,
                Direction = (ReadingDirection)r.Direction
            })
            .ToList();

        return TypedResults.Ok(rawReadings);
    }
}