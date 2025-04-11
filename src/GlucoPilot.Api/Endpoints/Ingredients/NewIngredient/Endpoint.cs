using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;

internal static class Endpoint
{
    internal static async Task<Results<Ok<NewIngredientResponse>, ValidationProblem>> HandleAsync(
        [FromBody] NewIngredientRequest request,
        [FromServices] IValidator<NewIngredientRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var ingredient = new Ingredient
        {
            Name = request.Name,
            Created = DateTimeOffset.UtcNow,
            Carbs = request.Carbs,
            Protein = request.Protein,
            Fat = request.Fat,
            Calories = request.Calories,
            Uom = request.Uom,
            UserId = userId,
        };

        ingredientRepository.Add(ingredient);

        var response = new NewIngredientResponse
        {
            Id = ingredient.Id,
            Created = ingredient.Created,
            Name = ingredient.Name,
            Carbs = ingredient.Carbs,
            Protein = ingredient.Protein,
            Fat = ingredient.Fat,
            Calories = ingredient.Calories,
            Uom = ingredient.Uom,
        };

        return TypedResults.Ok(response);
    }
}
