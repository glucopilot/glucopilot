using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Meals.UpdateMeal;

internal static class Endpoint
{
    internal static async Task<Results<Ok, NoContent, ValidationProblem>> HandleAsync(
        [FromBody] UpdateMealRequest request,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();
        var meal = await mealRepository.FindOneAsync(m => m.Id == request.Id && m.UserId == userId, new FindOptions() { IsAsNoTracking = true }).ConfigureAwait(false);

        if (meal is null)
        {
            throw new NotFoundException("MEAL_NOT_FOUND");
        }

        var ingredientIds = request.MealIngredients.Select(mi => mi.IngredientId).ToList();
        var missingIngredientExists = await ingredientRepository.AnyAsync(i => !ingredientIds.Contains(i.Id), cancellationToken).ConfigureAwait(false);

        if (missingIngredientExists)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        meal.Name = request.Name;

        meal.MealIngredients.Clear();
        foreach (var ingredientRequest in request.MealIngredients)
        {
            meal.MealIngredients.Add(new MealIngredient
            {
                Id = Guid.NewGuid(),
                MealId = meal.Id,
                IngredientId = ingredientRequest.IngredientId,
                Quantity = ingredientRequest.Quantity,
            });
        }

        await mealRepository.UpdateAsync(meal, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
