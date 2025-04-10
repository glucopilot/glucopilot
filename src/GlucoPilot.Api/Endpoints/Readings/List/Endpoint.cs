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

namespace GlucoPilot.Api.Endpoints.Readings.List;

internal static class Endpoint
{
    internal static async Task<Results<Ok<List<ReadingsResponse>>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
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

        if (currentUser.GetUserId() is null)
        {
            return TypedResults.Unauthorized();
        }

        var readings = repository.Find(r => r.UserId == currentUser.GetUserId() && r.Created >= request.From && r.Created <= request.To, new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(r => r.Created)
            .Select(r => new ReadingsResponse
            {
                UserId = r.UserId,
                Id = r.Id,
                Created = r.Created,
                GlucoseLevel = r.GlucoseLevel,
                Direction = r.Direction
            })
            .ToList();

        return TypedResults.Ok(readings);
    }
}
