using FluentValidation;
using GlucoPilot.Api.Endpoints.Treatments.NewTreatment;
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
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;

internal static class Endpoint
{
    internal static async Task<Results<Ok<UpdateTreatmentResponse>, NotFound, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateTreatmentRequest request,
        [FromServices] IValidator<UpdateTreatmentRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentRepository,
        [FromServices] IRepository<Reading> readingRepository,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        [FromServices] IRepository<Injection> injectionRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var treatment = await treatmentRepository
            .FindOneAsync(t => t.Id == id && t.UserId == userId, new FindOptions { IsAsNoTracking = false }, cancellationToken)
            .ConfigureAwait(false);

        if (treatment is null)
        {
            throw new NotFoundException("TREATMENT_NOT_FOUND");
        }

        Injection? injection = null;
        if (request.InjectionId is not null)
        {
            injection = injectionRepository
                .Find(i => i.Id == request.InjectionId && i.UserId == userId, new FindOptions { IsAsNoTracking = true })
                .Include(i => i.Insulin).FirstOrDefault();

            if (injection is null)
            {
                throw new NotFoundException("INJECTION_NOT_FOUND");
            }
        }

        var mealIds = request.Meals.Select(m => m.Id).ToList();
        var meals = mealRepository.Find(m => mealIds.Contains(m.Id) && m.UserId == userId, new FindOptions { IsAsNoTracking = true }).ToList();
        var invalidMealIds = mealIds.Except(meals.Select(m => m.Id)).ToList();

        if (invalidMealIds.Count > 0)
        {
            throw new NotFoundException("MEAL_NOT_FOUND");
        }

        var ingredientIds = request.Ingredients.Select(i => i.Id).ToList();
        var ingredients = ingredientRepository.Find(i => ingredientIds.Contains(i.Id) && i.UserId == userId, new FindOptions { IsAsNoTracking = true }).ToList();
        var invalidIngredientIds = ingredientIds.Except(ingredients.Select(i => i.Id)).ToList();

        if (invalidIngredientIds.Count > 0)
        {
            throw new NotFoundException("INGREDIENT_NOT_FOUND");
        }

        Reading? reading = null;
        if (request.ReadingId is not null)
        {
            reading = await readingRepository
                .FindOneAsync(r => r.Id == request.ReadingId && r.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
                .ConfigureAwait(false);
            if (reading is null)
            {
                throw new NotFoundException("READING_NOT_FOUND");
            }
        }

        treatment.ReadingId = request.ReadingId;
        treatment.Meals = request.Meals.Select(m => new TreatmentMeal
        {
            Id = Guid.NewGuid(),
            MealId = m.Id,
            TreatmentId = treatment.Id,
            Quantity = m.Quantity,
        }).ToList();
        treatment.Ingredients = request.Ingredients.Select(i => new TreatmentIngredient
        {
            Id = Guid.NewGuid(),
            IngredientId = i.Id,
            TreatmentId = treatment.Id,
            Quantity = i.Quantity,
        }).ToList();
        treatment.InjectionId = request.InjectionId;
        treatment.Updated = DateTimeOffset.UtcNow;

        await treatmentRepository.UpdateAsync(treatment, cancellationToken);

        var response = new UpdateTreatmentResponse
        {
            Id = treatment.Id,
            ReadingId = treatment.ReadingId,
            ReadingGlucoseLevel = reading?.GlucoseLevel,
            Meals = treatment.Meals.Select(m => new UpdateTreatmentMealResponse
            {
                Id = m.MealId,
                Name = m.Meal?.Name ?? "",
                Quantity = m.Quantity,
            }).ToList(),
            Ingredients = treatment.Ingredients.Select(i => new UpdateTreatmentIngredientResponse
            {
                Id = i.IngredientId,
                Name = i.Ingredient?.Name ?? "",
                Quantity = i.Quantity,
            }).ToList(),
            InjectionId = treatment.InjectionId,
            InsulinName = injection?.Insulin?.Name,
            InsulinUnits = injection?.Units,
            Updated = treatment.Updated
        };

        return TypedResults.Ok(response);
    }
}
