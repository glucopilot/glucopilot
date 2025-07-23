using FluentValidation;
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
using GlucoPilot.Api.Models;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListTreatmentsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListTreatmentsRequest request,
        [FromServices] IValidator<ListTreatmentsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        if (request.From is null)
        {
            request.From = DateTimeOffset.MinValue;
        }
        if (request.To is null)
        {
            request.To = DateTimeOffset.MaxValue;
        }

        var userId = currentUser.GetUserId();

        var treatment = treatmentRepository
            .GetAll(new FindOptions { IsAsNoTracking = true })
            .Where(t => t.UserId == userId && t.Created >= request.From && t.Created <= request.To)
            .OrderByDescending(t => t.Created)
            .Include(t => t.Meal)
            .ThenInclude(m => m.MealIngredients)
            .ThenInclude(mi => mi.Ingredient)
            .Include(t => t.Injection)
            .ThenInclude(i => i.Insulin)
            .Include(t => t.Reading)
            .ToList();

        var totalTreatments = await treatmentRepository.CountAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalTreatments / (double)request.PageSize);

        var response = new ListTreatmentsResponse
        {
            NumberOfPages = numberOfPages,
            Treatments = treatment.Select(t => new ListTreatmentResponse
            {
                Id = t.Id,
                Created = t.Created,
                Type = (TreatmentType)t.Type,
                Meal = t.Meal is not null
                    ? new ListTreatmentMealResponse
                    {
                        Id = t.Meal.Id,
                        Name = t.Meal.Name,
                        TotalCalories = t.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Calories * mi.Quantity),
                        TotalCarbs = t.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Carbs * mi.Quantity),
                        TotalProtein = t.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Protein * mi.Quantity),
                        TotalFat = t.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Fat * mi.Quantity),
                    }
                    : null,
                Injection = t.Injection is not null
                    ? new ListTreatmentInjectionResponse
                    {
                        Id = t.Injection.Id,
                        InsulinName = t.Injection.Insulin?.Name ?? "",
                        Units = t.Injection?.Units ?? 0,
                        Created = t.Injection?.Created ?? DateTimeOffset.MinValue,
                    }
                    : null,
                Reading = t.Reading is not null
                    ? new ListTreatmentReadingResponse
                    {
                        Id = t.Reading.Id,
                        GlucoseLevel = t.Reading.GlucoseLevel,
                    }
                    : null,
            }).ToList()
        };

        return TypedResults.Ok(response);
    }
}
