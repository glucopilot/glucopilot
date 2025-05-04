using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insulins.UpdateInsulin;

internal static class Endpoint
{
    internal static async Task<Results<Ok<UpdateInsulinResponse>, NotFound, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateInsulinRequest request,
        [FromServices] IValidator<UpdateInsulinRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var insulin = await insulinRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions() { IsAsNoTracking = false }, cancellationToken).ConfigureAwait(false);

        if (insulin is null)
        {
            throw new NotFoundException("INSULIN_NOT_FOUND");
        }

        insulin.Name = request.Name;
        insulin.Type = (Data.Enums.InsulinType)request.Type;
        insulin.Duration = request.Duration;
        insulin.Scale = request.Scale;
        insulin.PeakTime = request.PeakTime;
        insulin.Updated = DateTimeOffset.UtcNow;

        await insulinRepository.UpdateAsync(insulin, cancellationToken).ConfigureAwait(false);

        var response = new UpdateInsulinResponse
        {
            Id = insulin.Id,
            Name = insulin.Name,
            Type = (Models.InsulinType)insulin.Type,
            Duration = insulin.Duration,
            Scale = insulin.Scale,
            PeakTime = insulin.PeakTime,
            Updated = insulin.Updated
        };

        return TypedResults.Ok(response);
    }
}
