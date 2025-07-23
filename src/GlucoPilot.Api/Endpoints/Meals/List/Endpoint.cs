using FluentValidation;
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
using GlucoPilot.Api.Models;

namespace GlucoPilot.Api.Endpoints.Meals.List;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListMealsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListMealsRequest request,
        [FromServices] IValidator<ListMealsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> repository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var mealsQuery = repository.Find(m => m.UserId == userId, new FindOptions { IsAsNoTracking = true });

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLowerInvariant();
#pragma warning disable CA1862 // EF cannot use a query with a .Contains(StringComparison) within it.
            mealsQuery = mealsQuery.Where(m => m.Name.ToLower().Contains(searchLower));
#pragma warning restore CA1862 // EF cannot use a query with a .Contains(StringComparison) within it.
        }

        var meals = mealsQuery
            .OrderByDescending(m => m.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new GetMealResponse
            {
                Id = m.Id,
                Created = m.Created,
                Name = m.Name,
                MealIngredients = m.MealIngredients.Select(mi => new MealIngredientResponse
                {
                    Id = mi.Id,
                    Ingredient = mi.Ingredient != null ? new IngredientResponse
                    {
                        Id = mi.Ingredient!.Id,
                        Name = mi.Ingredient.Name,
                        Carbs = mi.Ingredient.Carbs,
                        Protein = mi.Ingredient.Protein,
                        Fat = mi.Ingredient.Fat,
                        Calories = mi.Ingredient.Calories,
                        Uom = (UnitOfMeasurement)mi.Ingredient.Uom
                    } : null,
                    Quantity = mi.Quantity
                }).ToList(),
                TotalCalories = m.MealIngredients.Sum(mi => mi.Ingredient == null ? 0 : mi.Ingredient.Calories * mi.Quantity),
                TotalCarbs = m.MealIngredients.Sum(mi => mi.Ingredient == null ? 0 : mi.Ingredient!.Carbs * mi.Quantity),
                TotalProtein = m.MealIngredients.Sum(mi => mi.Ingredient == null ? 0 : mi.Ingredient!.Protein * mi.Quantity),
                TotalFat = m.MealIngredients.Sum(mi => mi.Ingredient == null ? 0 : mi.Ingredient!.Fat * mi.Quantity)
            })
            .ToList();

        var totalMeals = await repository.CountAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalMeals / (double)request.PageSize);

        var response = new ListMealsResponse
        {
            NumberOfPages = numberOfPages,
            Meals = meals,
        };

        return TypedResults.Ok(response);
    }
}
