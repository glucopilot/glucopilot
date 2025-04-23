﻿using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
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

namespace GlucoPilot.Api.Endpoints.Meals.NewMeal;

internal static class Endpoint
{
    internal static async Task<Results<Created<NewMealResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewMealRequest request,
        [FromServices] IValidator<NewMealRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] IRepository<Ingredient> ingredientRepository)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var ingredientIds = request.MealIngredients.Select(x => x.IngredientId).ToList();
        var validIngredientIds = ingredientRepository
            .Find(x => ingredientIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        var invalidIngredientIds = ingredientIds.Except(validIngredientIds).ToList();
        if (invalidIngredientIds.Any())
        {
            throw new BadRequestException(Resources.ValidationMessages.IngredientIdInvalid);
        }

        var newMeal = new Meal
        {
            Name = request.Name,
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
            MealIngredients = [],
        };
        newMeal.MealIngredients = request.MealIngredients.Select(x => new MealIngredient()
        {
            Id = Guid.NewGuid(),
            MealId = newMeal.Id,
            IngredientId = x.IngredientId,
            Quantity = x.Quantity,
        }).ToList();

        mealRepository.Add(newMeal);

        var response = new NewMealResponse
        {
            Id = newMeal.Id,
            Name = newMeal.Name,
        };

        return TypedResults.Created($"/api/v1/meals/{newMeal.Id}", response);
    }
}
