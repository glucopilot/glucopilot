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

namespace GlucoPilot.Api.Endpoints.Pens.RemovePen;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Pen> penRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var pen = await penRepository.FindOneAsync(p => p.Id == id && p.UserId == userId, new FindOptions { IsAsNoTracking = false }, cancellationToken).ConfigureAwait(false);

        if (pen is null)
        {
            throw new NotFoundException(Resources.ValidationMessages.PenNotFound);
        }

        await penRepository.DeleteAsync(pen, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
