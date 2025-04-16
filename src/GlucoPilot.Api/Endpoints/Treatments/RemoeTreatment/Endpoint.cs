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

namespace GlucoPilot.Api.Endpoints.Treatments.RemoveTreatment;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound<ErrorResult>, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentRepository,
        CancellationToken canellationToken)
    {
        var userId = currentUser.GetUserId();

        var treatment = await treatmentRepository
            .FindOneAsync(t => t.Id == id && t.UserId == userId, new FindOptions { IsAsNoTracking = false }, canellationToken);

        if (treatment is null)
        {
            throw new NotFoundException("TREATMENT_NOT_FOUND");
        }

        treatmentRepository.Delete(treatment);

        return TypedResults.NoContent();
    }
}
