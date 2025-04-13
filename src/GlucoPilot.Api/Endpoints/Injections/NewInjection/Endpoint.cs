using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
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
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var newInjection = new Injection
        {
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
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
        };

        return TypedResults.Ok(response);
    }
}
