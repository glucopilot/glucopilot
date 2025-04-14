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

namespace GlucoPilot.Api.Endpoints.Injections.RemoveInjection;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Injection> injectionRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();
        var injection = await injectionRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions() { IsAsNoTracking = false }, cancellationToken).ConfigureAwait(false);

        if (injection is null)
        {
            throw new NotFoundException("INJECTION_NOT_FOUND");
        }

        await injectionRepository.DeleteAsync(injection, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
