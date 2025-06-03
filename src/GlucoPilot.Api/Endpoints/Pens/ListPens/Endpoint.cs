using FluentValidation;
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

namespace GlucoPilot.Api.Endpoints.Pens.ListPens;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListPensResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListPensRequest request,
        [FromServices] IValidator<ListPensRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Pen> pensRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var pens = pensRepository.Find(p => p.UserId == userId, new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(p => p.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PenResponse
            {
                Id = p.Id,
                Created = p.Created,
                Updated = p.Updated,
                Model = (Models.PenModel)p.Model,
                Colour = (Models.PenColour)p.Colour,
                Serial = p.Serial,
                InsulinId = p.InsulinId,
                InsulinName = p.Insulin != null ? p.Insulin.Name : "",
                StartTime = p.StartTime,
            })
            .ToList();

        var totalPens = await pensRepository.CountAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalPens / (double)request.PageSize);

        var response = new ListPensResponse
        {
            Pens = pens,
            NumberOfPages = numberOfPages,
        };

        return TypedResults.Ok(response);
    }
}
