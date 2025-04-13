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

namespace GlucoPilot.Api.Endpoints.Insulins.GetInsulin;

internal static class Endpoint
{
    internal static async Task<Results<Ok<GetInsulinResponse>, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var insulin = await insulinRepository.FindOneAsync(i => i.Id == id && (i.UserId == null || i.UserId == userId), new FindOptions { IsAsNoTracking = true }, cancellationToken);

        if (insulin is null)
        {
            throw new NotFoundException("INSULIN_NOT_FOUND");
        }

        var response = new GetInsulinResponse
        {
            Id = insulin.Id,
            Name = insulin.Name,
            Created = insulin.Created,
            Type = insulin.Type,
            Duration = insulin.Duration,
            Scale = insulin.Scale,
            PeakTime = insulin.PeakTime
        };

        return TypedResults.Ok(response);
    }
}
