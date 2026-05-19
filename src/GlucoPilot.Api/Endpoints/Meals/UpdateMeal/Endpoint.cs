using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        [FromServices] IRepository<MealIngredient> mealIngredientRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();
        var meal = mealRepository
            .Find(m => m.Id == request.Id && m.UserId == userId)
            .Include(m => m.MealIngredients)
            .FirstOrDefault();

        if (meal is null)
        {
            throw new NotFoundException("MEAL_NOT_FOUND");
        }

        var ingredientIds = request.MealIngredients.Select(mi => mi.IngredientId).Distinct().ToList();
        var existingIngredientCount = await ingredientRepository.CountAsync(i => ingredientIds.Contains(i.Id), cancellationToken).ConfigureAwait(false);

        if (existingIngredientCount != ingredientIds.Count)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        meal.Name = request.Name;
        meal.Updated = DateTimeOffset.UtcNow;

        var existingMealIngredientsById = meal.MealIngredients.ToDictionary(mi => mi.Id);
        var requestedExistingIds = new HashSet<Guid>(request.MealIngredients
            .Where(mi => mi.Id.HasValue)
            .Select(mi => mi.Id!.Value));

        var mealIngredientIdsToDelete = meal.MealIngredients
            .Where(mi => !requestedExistingIds.Contains(mi.Id))
            .Select(mi => mi.Id)
            .ToList();

        if (mealIngredientIdsToDelete.Count > 0)
        {
            await mealIngredientRepository.DeleteManyAsync(mi => mealIngredientIdsToDelete.Contains(mi.Id), cancellationToken).ConfigureAwait(false);
            meal.MealIngredients = meal.MealIngredients
                .Where(mi => !mealIngredientIdsToDelete.Contains(mi.Id))
                .ToList();
        }

        foreach (var ingredientRequest in request.MealIngredients)
        {
            if (ingredientRequest.Id.HasValue)
            {
                if (!existingMealIngredientsById.TryGetValue(ingredientRequest.Id.Value, out var existingMealIngredient))
                {
                    throw new NotFoundException("MEAL_INGREDIENT_NOT_FOUND");
                }

                existingMealIngredient.IngredientId = ingredientRequest.IngredientId;
                existingMealIngredient.Quantity = ingredientRequest.Quantity;
                continue;
            }

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
