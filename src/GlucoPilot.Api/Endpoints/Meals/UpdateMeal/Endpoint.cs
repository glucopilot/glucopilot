using FluentValidation;
using GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;
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
    internal static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] UpdateMealRequest request,
        [FromServices] IValidator<UpdateMealRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();
        var meal = await mealRepository.FindOneAsync(m => m.Id == request.Id && m.UserId == userId, new FindOptions() { IsAsNoTracking = true }).ConfigureAwait(false);

        if (meal is null)
        {
            throw new NotFoundException("MEAL_NOT_FOUND");
        }

        var ingredientIds = request.MealIngredients.Select(mi => mi.IngredientId).ToList();
        var existingIngredientCount = await ingredientRepository.CountAsync(i => ingredientIds.Contains(i.Id), cancellationToken).ConfigureAwait(false);

        if (existingIngredientCount != ingredientIds.Count)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        meal.Name = request.Name;
        meal.Updated = DateTimeOffset.UtcNow;

        meal.MealIngredients.Clear();
        meal.MealIngredients = request.MealIngredients
            .Select(ingredientRequest => new MealIngredient
            {
                Id = ingredientRequest.Id,
                MealId = meal.Id,
                IngredientId = ingredientRequest.IngredientId,
                Quantity = ingredientRequest.Quantity,
            })
            .ToList();

        await mealRepository.UpdateAsync(meal, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
