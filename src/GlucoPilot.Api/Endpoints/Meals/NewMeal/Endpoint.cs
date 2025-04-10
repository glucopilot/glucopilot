using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Meals.NewMeal;

internal static class Endpoint
{
    internal static async Task<Results<Created<NewMealResponse>, UnauthorizedHttpResult>> HandleAsync(
        [FromBody] NewMealRequest request,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> repository)
    {
        var userId = currentUser.GetUserId();

        var newMeal = new Meal
        {
            Name = request.Name,
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
            MealIngredients = request.MealIngredients,
        };
        repository.Add(newMeal);

        var response = new NewMealResponse
        {
            Id = newMeal.Id,
            Name = newMeal.Name,
        };

        return TypedResults.Created($"/meals/{newMeal.Id}", response);
    }
}
