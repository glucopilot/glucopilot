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

namespace GlucoPilot.Api.Endpoints.Injections.UpdateInjection;

internal static class Endpoint
{
    internal static async Task<Results<Ok<UpdateInjectionResponse>, BadRequest, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateInjectionRequest request,
        [FromServices] IValidator<UpdateInjectionRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Injection> injectionRepository,
        [FromServices] IRepository<Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var injection = await injectionRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions() { IsAsNoTracking = false }, cancellationToken).ConfigureAwait(false);

        if (injection is null)
        {
            throw new NotFoundException("INJECTION_NOT_FOUND");
        }

        var insulin = await insulinRepository.FindOneAsync(i => i.Id == request.InsulinId && i.UserId == userId, new FindOptions() { IsAsNoTracking = true }, cancellationToken).ConfigureAwait(false);
        if (insulin is null)
        {
            throw new NotFoundException("INSULIN_NOT_FOUND");
        }

        injection.Units = request.Units;
        injection.InsulinId = request.InsulinId;
        injection.Updated = DateTimeOffset.UtcNow;

        await injectionRepository.UpdateAsync(injection, cancellationToken).ConfigureAwait(false);

        var response = new UpdateInjectionResponse
        {
            Id = injection.Id,
            InsulinId = injection.InsulinId,
            InsulinName = insulin.Name,
            Units = injection.Units,
            Updated = injection.Updated,
        };

        return TypedResults.Ok(response);
    }
}
