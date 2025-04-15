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
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        if (request.InjectionId is not null)
        {
            var injection = await injectionRepository
                .FindOneAsync(i => i.Id == request.InjectionId && i.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
                .ConfigureAwait(false);

            if (injection is null)
            {
                throw new NotFoundException("INJECTION_NOT_FOUND");
            }
        }

        if (request.MealId is not null)
        {
            var meal = await mealRepository
                .FindOneAsync(m => m.Id == request.MealId && m.UserId == userId, new FindOptions { IsAsNoTracking = true }, cancellationToken)
                .ConfigureAwait(false);
            if (meal is null)
            {
                throw new NotFoundException("MEAL_NOT_FOUND");
            }
        }

        if (request.ReadingId is not null)
        {
            var reading = await readingRepository
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
            MealId = request.MealId,
            InjectionId = request.InjectionId,
            ReadingId = request.ReadingId,
        };

        await treatmentRepository.AddAsync(treatment);

        var response = new NewTreatmentResponse
        {
            Id = treatment.Id,
            Created = treatment.Created,
            MealId = treatment.MealId,
            InjectionId = treatment.InjectionId,
            ReadingId = treatment.ReadingId,
        };

        return TypedResults.Ok(response);
    }
}
