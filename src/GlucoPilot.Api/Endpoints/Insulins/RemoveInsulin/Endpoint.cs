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

namespace GlucoPilot.Api.Endpoints.Insulins.RemoveInsulin;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IRepository<Insulin> insulinRepository,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var insulin = await insulinRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken).ConfigureAwait(false);
        if (insulin is null)
        {
            throw new NotFoundException("INSULIN_NOT_FOUND");
        }

        await insulinRepository.DeleteAsync(insulin, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
