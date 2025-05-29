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

namespace GlucoPilot.Api.Endpoints.Pens.NewPen;

internal static class Endpoint
{
    internal static async Task<Results<Created<NewPenResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewPenRequest request,
        [FromServices] IValidator<NewPenRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Pen> penRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }
        var userId = currentUser.GetUserId();

        var newPen = new Pen
        {
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
            InsulinId = request.InsulinId,
            Model = (Data.Enums.PenModel)request.Model,
            Colour = (Data.Enums.PenColour)request.Colour,
            Serial = request.Serial,
        };
        if (request.StartTime.HasValue)
        {
            newPen.StartTime = request.StartTime.Value;
        }

        await penRepository.AddAsync(newPen, cancellationToken).ConfigureAwait(false);

        var response = new NewPenResponse
        {
            Id = newPen.Id,
            InsulinId = newPen.InsulinId,
            Model = (Models.PenModel)newPen.Model,
            Colour = (Models.PenColour)newPen.Colour,
            Serial = newPen.Serial,
            Created = newPen.Created,
            StartTime = newPen.StartTime,
        };

        return TypedResults.Created($"/pens/{newPen.Id}", response);
    }
}
