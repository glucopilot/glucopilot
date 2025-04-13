using FluentValidation;
using GlucoPilot.Api.Endpoints.Insulins.GetInsulin;
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

namespace GlucoPilot.Api.Endpoints.Insulins.List;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListInsulinsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListInsulinsRequest request,
        [FromServices] IValidator<ListInsulinsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Insulin> repository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var insulins = repository.Find(i => i.UserId == userId && (request.Type == null || i.Type == request.Type), new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(i => i.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new GetInsulinResponse
            {
                Id = i.Id,
                Created = i.Created,
                Name = i.Name,
                Type = i.Type,
                Duration = i.Duration,
                Scale = i.Scale,
                PeakTime = i.PeakTime,
            })
            .ToList();

        var totalInsulins = await repository.CountAsync(i => i.UserId == userId && (request.Type == null || i.Type == request.Type), cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalInsulins / (double)request.PageSize);

        var response = new ListInsulinsResponse
        {
            NumberOfPages = numberOfPages,
            Insulins = insulins,
        };

        return TypedResults.Ok(response);
    }
}
