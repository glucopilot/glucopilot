using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Readings.NewReading;

internal static class Endpoint
{
    internal static async Task<Results<Ok, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewReadingRequest request,
        [FromServices] IValidator<NewReadingRequest> validator,
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

        var reading = new Reading
        {
            UserId = currentUser.GetUserId()!.Value,
            Created = request.Created,
            GlucoseLevel = request.GlucoseLevel,
            Direction = ReadingDirection.NotComputable,
        };

        await repository.AddAsync(reading, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok();
    }
}
