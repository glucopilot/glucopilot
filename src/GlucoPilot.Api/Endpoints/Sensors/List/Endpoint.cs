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

namespace GlucoPilot.Api.Endpoints.Sensors.List;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ListSensorsResponse>, UnauthorizedHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] ListSensorsRequest request,
        [FromServices] IValidator<ListSensorsRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Sensor> sensorRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        var sensors = sensorRepository.Find(s => s.UserId == userId, new FindOptions { IsAsNoTracking = true })
            .OrderByDescending(s => s.Created)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ListSensorResponse
            {
                Id = s.Id,
                SensorId = s.SensorId,
                Started = s.Started,
                Expires = s.Expires,
            })
            .ToList();

        var totalSensors = await sensorRepository.CountAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
        var numberOfPages = (int)Math.Ceiling(totalSensors / (double)request.PageSize);

        var response = new ListSensorsResponse
        {
            Sensors = sensors,
            NumberOfPages = numberOfPages
        };

        return TypedResults.Ok(response);
    }
}
