using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

internal static class Endpoint
{
    internal static async Task<Results<Ok<NewTreatmentResponse>, NotFound, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewTreatmentRequest request,
        [FromServices] IValidator<NewTreatmentRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentRepository,
        [FromServices] IRepository<Reading> readingRepository,
        [FromServices] IRepository<Meal> mealRepository,
        [FromServices] IRepository<Ingredient> ingredientRepository,
        [FromServices] IRepository<Injection> injectionRepository,
        [FromServices] IRepository<Insulin> insulinRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        Injection? injection = null;

        if (request.Injection is not null)
        {
            var insulin = await insulinRepository.FindOneAsync(i => i.Id == request.Injection.InsulinId && (i.UserId == userId || i.UserId == null), new FindOptions { IsAsNoTracking = true }, cancellationToken).ConfigureAwait(false);
            if (insulin is null)
            {
                throw new NotFoundException("INSULIN_NOT_FOUND");
            }

            injection = new Injection
            {
                UserId = userId,
                Created = request.Injection.Created,
                InsulinId = request.Injection.InsulinId,
                Units = request.Injection.Units,
            };

            await injectionRepository.AddAsync(injection, cancellationToken).ConfigureAwait(false);

            injection.Insulin = insulin;
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

        var treatment = new Treatment
        {
            UserId = userId,
            Created = request.Created ?? DateTimeOffset.UtcNow,
            Meals = [],
            Ingredients = [],
            InjectionId = injection?.Id,
            ReadingId = reading?.Id,
        };
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

        await treatmentRepository.AddAsync(treatment, cancellationToken);

        var response = new NewTreatmentResponse
        {
            Id = treatment.Id,
            Created = treatment.Created,
            Updated = treatment.Updated,
            Meals = treatment.Meals.Select(tm => new NewTreatmentMealResponse
            {
                Id = tm.MealId,
                Name = meals.Where(m => m.Id == tm.MealId).FirstOrDefault()?.Name ?? "",
                Quantity = tm.Quantity,
            }).ToList(),
            Ingredients = treatment.Ingredients.Select(ti => new NewTreatmentIngredientResponse
            {
                Id = ti.IngredientId,
                Name = ingredients.Where(i => i.Id == ti.IngredientId).FirstOrDefault()?.Name ?? "",
                Quantity = ti.Quantity,
            }).ToList(),
            InjectionId = injection?.Id,
            InsulinName = injection?.Insulin?.Name,
            InsulinUnits = injection?.Units,
            ReadingId = reading?.Id,
            ReadingGlucoseLevel = reading?.GlucoseLevel,
        };

        return TypedResults.Ok(response);
    }
}
