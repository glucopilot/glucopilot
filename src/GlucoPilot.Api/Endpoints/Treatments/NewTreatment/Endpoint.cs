using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [FromServices] IRepository<Injection> injectionRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        Injection? injection = null;

        if (request.InjectionId is not null)
        {
            injection = await injectionRepository
                .FindOneAsync(i => i.Id == request.InjectionId && i.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
                .ConfigureAwait(false);

            if (injection is null)
            {
                throw new NotFoundException("INJECTION_NOT_FOUND");
            }
        }

        Meal? meal = null;

        if (request.MealId is not null)
        {
            meal = await mealRepository
                .FindOneAsync(m => m.Id == request.MealId && m.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
                .ConfigureAwait(false);
            if (meal is null)
            {
                throw new NotFoundException("MEAL_NOT_FOUND");
            }
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
            MealId = meal?.Id,
            InjectionId = injection?.Id,
            ReadingId = reading?.Id,
        };

        await treatmentRepository.AddAsync(treatment, cancellationToken);

        var response = new NewTreatmentResponse
        {
            Id = treatment.Id,
            Created = treatment.Created,
            MealId = meal?.Id,
            MealName = meal?.Name,
            InjectionId = injection?.Id,
            InsulinName = injection?.Insulin?.Name,
            InsulinUnits = injection?.Units,
            ReadingId = reading?.Id,
            ReadingGlucoseLevel = reading?.GlucoseLevel,
        };

        return TypedResults.Ok(response);
    }
}
