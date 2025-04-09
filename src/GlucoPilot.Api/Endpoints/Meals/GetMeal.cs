using GlucoPilot.Api.Models;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Meals;

internal static class GetMeal
{
    internal static async Task<Results<Ok<MealResponse>, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromQuery] Guid mealId,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> repository)
    {
        var currentUserId = currentUser.GetUserId();

        if (currentUserId is null)
        {
            return TypedResults.Unauthorized();
        }

        var meal = repository
            .Find(m => m.UserId == currentUserId && m.Id == mealId, new FindOptions { IsAsNoTracking = true })
            .Include(m => m.MealIngredients)
            .ThenInclude(mi => mi.Ingredient).FirstOrDefault();

        if (meal is null)
        {
            return TypedResults.NotFound();
        }

        var response = new MealResponse
        {
            Id = meal.Id,
            Name = meal.Name,
            Created = meal.Created,
            MealIngredients = meal.MealIngredients.Select(mi => new MealIngredientResponse
            {
                Id = mi.Id,
                Quantity = mi.Quantity,
                Ingredient = mi.Ingredient is not null ? new IngredientResponse
                {
                    Id = mi.Ingredient.Id,
                    Name = mi.Ingredient.Name,
                    Carbs = mi.Ingredient.Carbs,
                    Protein = mi.Ingredient.Protein,
                    Fat = mi.Ingredient.Fat,
                    Calories = mi.Ingredient.Calories,
                    Uom = mi.Ingredient.Uom
                } : null
            }).ToList()
        };
        return TypedResults.Ok(response);
    }
}
