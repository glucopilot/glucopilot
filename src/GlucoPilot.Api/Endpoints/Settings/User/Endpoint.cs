using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Api.Endpoints.Settings.User;

internal static class Endpoint
{
    internal static async Task<Results<Ok<UserSettingsResponse>, UnauthorizedHttpResult>> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Data.Entities.User> userRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        var user = await userRepository.FindOneAsync(s => s.Id == userId, new FindOptions { IsAsNoTracking = false, IsIgnoreAutoIncludes = true }, cancellationToken)
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

        var response = new UserSettingsResponse
        {
            Email = user.Email,
            GlucoseProvider = (user is Patient patient) ? (GlucoseProvider?)patient.GlucoseProvider : null,
            GlucoseUnitOfMeasurement = (GlucoseUnitOfMeasurement)user.Settings.GlucoseUnitOfMeasurement,
            LowSugarThreshold = user.Settings.LowSugarThreshold,
            HighSugarThreshold = user.Settings.HighSugarThreshold,
            DailyCalorieTarget = user.Settings.DailyCalorieTarget,
            DailyCarbTarget = user.Settings.DailyCarbTarget,
            DailyProteinTarget = user.Settings.DailyProteinTarget,
            DailyFatTarget = user.Settings.DailyFatTarget,
        };

        return TypedResults.Ok(response);
    }
}