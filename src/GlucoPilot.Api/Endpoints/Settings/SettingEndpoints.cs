using System.Diagnostics.CodeAnalysis;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Api.Endpoints.Settings;

[ExcludeFromCodeCoverage]
internal static class SettingEndpoints
{
    internal static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/settings")
            .WithTags("Settings");

        group.MapGet("/user", User.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("GetUserSettings")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPatch("/user", PatchUser.Endpoint.HandleAsync)
            .HasApiVersion(1.0)
            .WithName("UpdateUserSettings")
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }
}