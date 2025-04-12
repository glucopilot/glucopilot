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

namespace GlucoPilot.Api.Endpoints.Ingredients.RemoveIngredient;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var ingredient = await ingredientRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken);

        if (ingredient is null)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        await ingredientRepository.DeleteAsync(ingredient, cancellationToken);

        return TypedResults.NoContent();
    }
}
