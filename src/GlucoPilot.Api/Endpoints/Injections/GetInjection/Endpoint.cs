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

namespace GlucoPilot.Api.Endpoints.Injections.GetInjection;

internal static class Endpoint
{
    internal static async Task<Results<Ok<GetInjectionResponse>, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Injection> injectionsRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var injection = await injectionsRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken).ConfigureAwait(false);
        if (injection is null)
        {
            throw new NotFoundException("INJECTION_NOT_FOUND");
        }

        var response = new GetInjectionResponse
        {
            Id = injection.Id,
            Created = injection.Created,
            InsulinId = injection.InsulinId,
            Units = injection.Units,
            InsulinName = injection.Insulin!.Name,
        };

        return TypedResults.Ok(response);
    }
}
