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
using GlucoPilot.Api.Models;

namespace GlucoPilot.Api.Endpoints.Treatments.GetTreatment;

internal static class Endpoint
{
    internal static async Task<Results<Ok<GetTreatmentResponse>, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> treatmentRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var treatment = treatmentRepository
            .GetAll(new FindOptions { IsAsNoTracking = true })
            .Where(t => t.Id == id && t.UserId == userId)
            .Include(t => t.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(t => t.Meals)
            .ThenInclude(m => m.Meal)
            .ThenInclude(m => m.MealIngredients)
            .ThenInclude(mi => mi.Ingredient)
            .Include(t => t.Injection)
            .ThenInclude(i => i.Insulin)
            .Include(t => t.Reading)
            .AsSplitQuery()
            .FirstOrDefault();

        if (treatment is null)
        {
            throw new NotFoundException("TREATMENT_NOT_FOUND");
        }

        return TypedResults.Ok(new GetTreatmentResponse
        {
            Id = treatment.Id,
            Created = treatment.Created,
            Updated = treatment.Updated,
            Type = (TreatmentType)treatment.Type,
            Meals = treatment.Meals.Select(m => new GetTreatmentMealResponse
            {
                Id = m.Meal.Id,
                Name = m.Meal.Name,
                TotalCalories = m.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Calories * mi.Quantity),
                TotalCarbs = m.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Carbs * mi.Quantity),
                TotalProtein = m.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Protein * mi.Quantity),
                TotalFat = m.Meal.MealIngredients.Sum(mi => mi.Ingredient is null ? 0 : mi.Ingredient.Fat * mi.Quantity),
                Quantity = m.Quantity,
            }).ToList(),
            Ingredients = treatment.Ingredients.Select(i => new GetTreatmentIngredientResponse
            {
                Id = i.Ingredient.Id,
                Name = i.Ingredient.Name,
                Quantity = i.Quantity,
                Uom = (UnitOfMeasurement)i.Ingredient.Uom,
                Carbs = i.Ingredient.Carbs,
                Protein = i.Ingredient.Protein,
                Fat = i.Ingredient.Fat,
                Calories = i.Ingredient.Calories,
            }).ToList(),
            Injection = treatment.Injection is not null
            ? new GetTreatmentInjectionResponse
            {
                Id = treatment.Injection.Id,
                InsulinName = treatment.Injection.Insulin?.Name ?? "",
                Units = treatment.Injection.Units,
            } : null,
            Reading = treatment.Reading is not null
            ? new GetTreatmentReadingResponse
            {
                Id = treatment.Reading.Id,
                GlucoseLevel = treatment.Reading.GlucoseLevel,
            } : null,

        });
    }
}
