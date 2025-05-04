using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Api.Endpoints.Insights.InsulinToCarbRatio;

internal static class Endpoint
{
    internal static async Task<Results<Ok<decimal?>, ValidationProblem, UnauthorizedHttpResult>> HandleAsync(
        [AsParameters] InsulinToCarbRatioRequest request,
        [FromServices] IValidator<InsulinToCarbRatioRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> repository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var insulinToCarbs = repository.Find(t =>
                    t.Meal != null && t.Meal.MealIngredients.Count > 0 && t.Injection != null && t.UserId == userId &&
                    t.Created >= request.From &&
                    t.Created <= request.To,
                new FindOptions { IsAsNoTracking = true }).Include(t => t.Injection).Include(t => t.Meal)
            .ThenInclude(m => m!.MealIngredients).ThenInclude(mi => mi.Ingredient)
            .AsEnumerable()
            .Average(x => x.InsulinToCarbRatio);

        return TypedResults.Ok(insulinToCarbs);
    }
}