using FluentValidation;
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

namespace GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;

internal static class Endpoint
{
    internal static async Task<Results<Ok<UpdateIngredientResponse>, NotFound, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateIngredientRequest request,
        [FromServices] IValidator<UpdateIngredientRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var ingredient = await ingredientRepository.FindOneAsync(i => i.Id == id && i.UserId == userId, new FindOptions() { IsAsNoTracking = true }).ConfigureAwait(false);

        if (ingredient is null)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        ingredient.Name = request.Name;
        ingredient.Carbs = request.Carbs;
        ingredient.Protein = request.Protein;
        ingredient.Fat = request.Fat;
        ingredient.Calories = request.Calories;
        ingredient.Uom = request.Uom;

        await ingredientRepository.UpdateAsync(ingredient, cancellationToken).ConfigureAwait(false);

        var response = new UpdateIngredientResponse
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Carbs = ingredient.Carbs,
            Protein = ingredient.Protein,
            Fat = ingredient.Fat,
            Calories = ingredient.Calories,
            Uom = ingredient.Uom
        };

        return TypedResults.Ok(response);
    }
}
