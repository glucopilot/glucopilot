using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insights.TotalNutrition
{
    internal static class Endpoint
    {
        internal static async Task<Results<Ok<TotalNutritionResponse>, ValidationProblem, UnauthorizedHttpResult>> HandleAsync(
            [AsParameters] TotalNutritionRequest request,
            [FromServices] IValidator<TotalNutritionRequest> validator,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<Treatment> repository,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUser.GetUserId();
            var to = request.To ?? DateTimeOffset.UtcNow;
            var from = request.From ?? to.AddDays(-7);

            var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            var treatments = repository
                .GetAll(new FindOptions { IsAsNoTracking = true })
                .Where(t => t.UserId == userId && t.Created >= request.From && t.Created <= request.To)
                .OrderByDescending(t => t.Created)
                .Include(t => t.Ingredients)
                .ThenInclude(i => i.Ingredient)
                .Include(t => t.Meals)
                .ThenInclude(m => m.Meal)
                .ThenInclude(m => m.MealIngredients)
                .ThenInclude(mi => mi.Ingredient)
                .Include(t => t.Injection)
                .AsSplitQuery()
                .ToList();

            var mealIngredients = treatments
                .SelectMany(t => t.Meals)
                .Where(tm => tm.Meal != null)
                .SelectMany(tm => tm.Meal!.MealIngredients)
                .Where(mi => mi.Ingredient != null);

            var ingredients = treatments
                .SelectMany(t => t.Ingredients)
                .Where(i => i.Ingredient != null);

            var response = new TotalNutritionResponse
            {
                TotalCalories = mealIngredients.Sum(mi => (mi.Ingredient?.Calories ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Calories ?? 0) * i.Quantity),
                TotalCarbs = mealIngredients.Sum(mi => (mi.Ingredient?.Carbs ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Carbs ?? 0) * i.Quantity),
                TotalProtein = mealIngredients.Sum(mi => (mi.Ingredient?.Protein ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Protein ?? 0) * i.Quantity),
                TotalFat = mealIngredients.Sum(mi => (mi.Ingredient?.Fat ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Fat ?? 0) * i.Quantity)
            };

            return TypedResults.Ok(response);
        }
    }
}
