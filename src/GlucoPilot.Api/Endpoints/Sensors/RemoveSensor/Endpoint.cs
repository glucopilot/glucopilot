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

namespace GlucoPilot.Api.Endpoints.Sensors.RemoveSensor;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, NotFound<ErrorResult>, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Sensor> sensorRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var sensor = await sensorRepository
            .FindOneAsync(s => s.Id == id && s.UserId == userId, new FindOptions { IsAsNoTracking = false }, cancellationToken).ConfigureAwait(false);

        if (sensor is null)
        {
            throw new NotFoundException("SENSOR_NOT_FOUND");
        }

        await sensorRepository.DeleteAsync(sensor, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
