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

namespace GlucoPilot.Api.Endpoints.Meals.Nutrition;

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

        var meals = repository
            .Find(m => m.UserId == userId, new FindOptions { IsAsNoTracking = true })
            .Include(m => m.Meal)
            .ThenInclude(mi => mi.MealIngredients)
            .ThenInclude(mi => mi.Ingredient)
            .Where(t => t.Meal != null && t.Created >= request.From && t.Created <= request.To)
            .Select(t => t.Meal)
            .ToList();

        var response = new NutritionResponseModel
        {
            TotalCalories = meals.Sum(m =>
                m.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Calories * mi.Quantity)),
            TotalCarbs = meals.Sum(m =>
                m.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Carbs * mi.Quantity)),
            TotalProtein = meals.Sum(m =>
                m.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Protein * mi.Quantity)),
            TotalFat = meals.Sum(m =>
                m.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Fat * mi.Quantity))
        };

        return TypedResults.Ok(response);
    }
}