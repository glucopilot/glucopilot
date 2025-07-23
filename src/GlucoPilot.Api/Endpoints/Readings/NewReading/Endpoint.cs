using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Api.Models;
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
    internal static async Task<Results<Created<NewReadingResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
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

        var userId = currentUser.GetUserId();

        var reading = new Reading
        {
            UserId = userId,
            Created = request.Created,
            GlucoseLevel = request.GlucoseLevel,
            Direction = (Data.Enums.ReadingDirection)ReadingDirection.NotComputable,
        };

        await repository.AddAsync(reading, cancellationToken).ConfigureAwait(false);

        var response = new NewReadingResponse
        {
            Id = reading.Id,
            Created = reading.Created,
            GlucoseLevel = reading.GlucoseLevel,
        };

        return TypedResults.Created((string?)null, response);
    }
}
