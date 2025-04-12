using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Insulins.NewInsulin;

internal static class Endpoint
{
    internal static async Task<Results<Created<NewInsulinResponse>, ValidationProblem>> HandleAsync(
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
            PeakTime = request.PeakTime,
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
        };

        insulinsRepository.Add(insulin);

        var response = new NewInsulinResponse
        {
            Id = insulin.Id,
            Name = request.Name,
            Type = request.Type,
            Duration = request.Duration,
            Scale = request.Scale,
            PeakTime = request.PeakTime,
            Created = DateTimeOffset.UtcNow,
        };

        return TypedResults.Created($"/insulins/{insulin.Id}", response);
    }
}
