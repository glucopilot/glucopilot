using FluentValidation;
using GlucoPilot.Api.Endpoints.Meals.GetMeal;
using GlucoPilot.Api.Models;
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
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var meals = repository.Find(m => m.UserId == userId, new FindOptions { IsAsNoTracking = true })
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
                    Ingredient = new IngredientResponse
                    {
                        Id = mi.Ingredient!.Id,
                        Name = mi.Ingredient.Name,
                        Carbs = mi.Ingredient.Carbs,
                        Protein = mi.Ingredient.Protein,
                        Fat = mi.Ingredient.Fat,
                        Calories = mi.Ingredient.Calories,
                        Uom = mi.Ingredient.Uom
                    },
                    Quantity = mi.Quantity
                }).ToList(),
                TotalCalories = m.MealIngredients.Sum(mi => mi.Ingredient!.Calories * mi.Quantity),
                TotalCarbs = m.MealIngredients.Sum(mi => mi.Ingredient!.Carbs * mi.Quantity),
                TotalProtein = m.MealIngredients.Sum(mi => mi.Ingredient!.Protein * mi.Quantity),
                TotalFat = m.MealIngredients.Sum(mi => mi.Ingredient!.Fat * mi.Quantity)
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
