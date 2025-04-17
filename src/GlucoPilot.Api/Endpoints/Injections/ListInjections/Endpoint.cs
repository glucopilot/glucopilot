using FluentValidation;
using GlucoPilot.Api.Endpoints.Injections.GetInjection;
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

namespace GlucoPilot.Api.Endpoints.Injections.ListInjections;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListInjectionsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListInjectionsRequest request,
        [FromServices] IValidator<ListInjectionsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Injection> repository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var injections = repository.Find(i => i.UserId == userId, new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(i => i.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new GetInjectionResponse
            {
                Id = i.Id,
                Created = i.Created,
                InsulinId = i.InsulinId,
                InsulinName = i.Insulin!.Name,
                Units = i.Units,
                Updated = i.Updated,
            })
            .ToList();

        var totalInjections = await repository.CountAsync(i => i.UserId == userId, cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalInjections / (double)request.PageSize);

        var response = new ListInjectionsResponse
        {
            NumberOfPages = numberOfPages,
            Injections = injections,
        };

        return TypedResults.Ok(response);
    }
}
