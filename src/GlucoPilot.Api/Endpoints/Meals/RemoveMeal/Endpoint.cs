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

namespace GlucoPilot.Api.Endpoints.Meals.RemoveMeal;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound>> HandleAsync(
        [FromRoute] Guid mealId,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var meal = await mealRepository.FindOneAsync(m => m.Id == mealId && m.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken);

        if (meal is null)
        {
            throw new NotFoundException("MEAL_NOT_FOUND");
        }

        await mealRepository.DeleteAsync(meal, cancellationToken);

        return TypedResults.NoContent();
    }
}
