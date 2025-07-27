using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Api.Endpoints.Treatments.Nutrition;

internal static class Endpoint
{
    internal static async Task<Results<Ok<NutritionResponseModel>, UnauthorizedHttpResult, ValidationProblem>>
        HandleAsync(
            [AsParameters] NutritionRequestModel request,
            [FromServices] IValidator<NutritionRequestModel> validator,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<Treatment> repository,
            CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var treatments = repository
            .Find(m => m.UserId == userId, new FindOptions { IsAsNoTracking = true })
            .Include(t => t.Ingredients)
            .Include(m => m.Meals)
            .ThenInclude(m => m.Meal)
            .ThenInclude(mi => mi!.MealIngredients)
            .ThenInclude(mi => mi.Ingredient)
            .Where(t => (t.Meals.Count > 0 || t.Ingredients.Count > 0) && t.Created >= request.From && t.Created <= request.To)
            .AsSplitQuery()
            .ToList();

        var mealIngredients = treatments
            .SelectMany(t => t.Meals)
            .Where(tm => tm.Meal != null)
            .SelectMany(tm => tm.Meal!.MealIngredients)
            .Where(mi => mi.Ingredient != null);

        var ingredients = treatments
            .SelectMany(t => t.Ingredients)
            .Where(i => i != null);

        var response = new NutritionResponseModel
        {
            TotalCalories = mealIngredients.Sum(mi => (mi.Ingredient?.Calories ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Calories ?? 0) * i.Quantity),
            TotalCarbs = mealIngredients.Sum(mi => (mi.Ingredient?.Carbs ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Carbs ?? 0) * i.Quantity),
            TotalProtein = mealIngredients.Sum(mi => (mi.Ingredient?.Protein ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Protein ?? 0) * i.Quantity),
            TotalFat = mealIngredients.Sum(mi => (mi.Ingredient?.Fat ?? 0) * mi.Quantity) + ingredients.Sum(i => (i.Ingredient?.Fat ?? 0) * i.Quantity)
        };

        return TypedResults.Ok(response);
    }
}