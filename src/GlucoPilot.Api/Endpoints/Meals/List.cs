using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Meals;

internal static class List
{
    internal static async Task<Results<Ok<ListMealsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListMealsRequest request,
        [FromServices] IValidator<ListMealsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Meal> repository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        if (currentUser.GetUserId() is null)
        {
            throw new UnauthorizedException("PATIENT_NOT_FOUND");
        }

        var meals = repository.Find(m => m.UserId == currentUser.GetUserId(), new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(m => m.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MealResponse
            {
                Id = m.Id,
                Created = m.Created,
                Name = m.Name,
            })
            .ToList();

        var totalMeals = await repository.CountAsync(m => m.UserId == currentUser.GetUserId(), cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalMeals / (double)request.PageSize);

        var response = new ListMealsResponse
        {
            NumberOfPages = numberOfPages,
            Meals = meals,
        };

        return TypedResults.Ok(response);
    }
}
