using FluentValidation;
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
            .FindOneAsync(t => t.Id == id && t.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
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

        treatment.ReadingId = request.ReadingId;
        treatment.MealId = request.MealId;
        treatment.InjectionId = request.InjectionId;

        await treatmentRepository.UpdateAsync(treatment, cancellationToken);

        var response = new UpdateTreatmentResponse
        {
            Id = treatment.Id,
            ReadingId = treatment.ReadingId,
            ReadingGlucoseLevel = reading?.GlucoseLevel,
            MealId = treatment.MealId,
            MealName = meal?.Name,
            InjectionId = treatment.InjectionId,
            InsulinName = injection?.Insulin?.Name,
            InsulinUnits = injection?.Units,
        };

        return TypedResults.Ok(response);
    }
}
