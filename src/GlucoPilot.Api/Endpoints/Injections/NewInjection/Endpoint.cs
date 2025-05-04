using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Injections.NewInjection;

internal static class Endpoint
{
    internal static async Task<Results<Ok<NewInjectionResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewInjectionRequest request,
        [FromServices] IValidator<NewInjectionRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Injection> injectionsRepository,
        [FromServices] IRepository<Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var insulin = await insulinRepository.FindOneAsync(i => i.Id == request.InsulinId && (i.UserId == userId || i.UserId == null), new FindOptions { IsAsNoTracking = true }, cancellationToken).ConfigureAwait(false);
        if (insulin is null)
        {
            throw new NotFoundException("INSULIN_NOT_FOUND");
        }

        var newInjection = new Injection
        {
            UserId = userId,
            Created = request.Created,
            InsulinId = request.InsulinId,
            Units = request.Units,
        };

        injectionsRepository.Add(newInjection);

        var response = new NewInjectionResponse
        {
            Id = newInjection.Id,
            Created = newInjection.Created,
            InsulinId = newInjection.InsulinId,
            Units = newInjection.Units,
            Updated = newInjection.Updated,
        };

        return TypedResults.Ok(response);
    }
}
