using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.Pens.NewPen;

internal static class Endpoint
{
    internal static async Task<Results<Created<NewPenResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [FromBody] NewPenRequest request,
        [FromServices] IValidator<NewPenRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Pen> penRepository)
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
        await penRepository.AddAsync(newPen).ConfigureAwait(false);

        var response = new NewPenResponse
        {
            Id = newPen.Id,
            InsulinId = newPen.InsulinId,
            Model = (Models.PenModel)newPen.Model,
            Colour = (Models.PenColour)newPen.Colour,
            Serial = newPen.Serial,
            Created = newPen.Created
        };

        return TypedResults.Created($"/pens/{newPen.Id}", response);
    }
}
