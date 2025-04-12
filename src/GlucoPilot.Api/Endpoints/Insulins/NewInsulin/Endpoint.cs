using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insulins.NewInsulin;

internal static class Endpoint
{
    internal static async Task<Results<Ok<NewInsulinResponse>, ValidationProblem>> HandleAsync(
        [FromBody] NewInsulinRequest request,
        [FromServices] IValidator<NewInsulinRequest> validator,
        [FromServices] IRepository<Insulin> insulinsRepository,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var insulin = new Insulin
        {
            Name = request.Name,
            Type = request.Type,
            Duration = request.Duration,
            Scale = request.Scale,
            UserId = userId,
        };

        insulinsRepository.Add(insulin);

        var response = new NewInsulinResponse
        {
            Id = insulin.Id,
            Name = insulin.Name,
            Type = insulin.Type,
            Duration = insulin.Duration,
            Scale = insulin.Scale,
        };

        return TypedResults.Ok(response);
    }
}
