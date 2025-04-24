using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Api.Endpoints.Settings.PatchUser;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, UnauthorizedHttpResult>> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Data.Entities.User> userRepository,
        [FromBody] UserSettingsPatchRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var user = await userRepository.FindOneAsync(s => s.Id == userId,
                new FindOptions { IsAsNoTracking = false, IsIgnoreAutoIncludes = true }, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            throw new UnauthorizedException("USER_NOT_LOGGED_IN");
        }

        if (user.Settings is null)
        {
            user.Settings = new UserSettings();
            await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        }

        if (request.GlucoseUnitOfMeasurement is not null)
        {
            user.Settings.GlucoseUnitOfMeasurement =
                (Data.Enums.GlucoseUnitOfMeasurement)request.GlucoseUnitOfMeasurement.Value;
        }

        if (request.LowSugarThreshold is not null)
        {
            user.Settings.LowSugarThreshold = request.LowSugarThreshold.Value;
        }

        if (request.HighSugarThreshold is not null)
        {
            user.Settings.HighSugarThreshold = request.HighSugarThreshold.Value;
        }

        if (request.DailyCalorieTarget is not null)
        {
            user.Settings.DailyCalorieTarget = request.DailyCalorieTarget.Value;
        }

        if (request.DailyCarbTarget is not null)
        {
            user.Settings.DailyCarbTarget = request.DailyCarbTarget.Value;
        }

        if (request.DailyProteinTarget is not null)
        {
            user.Settings.DailyProteinTarget = request.DailyProteinTarget.Value;
        }

        if (request.DailyFatTarget is not null)
        {
            user.Settings.DailyFatTarget = request.DailyFatTarget.Value;
        }

        await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}