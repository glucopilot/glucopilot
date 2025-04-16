using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Identity.Extensions;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Identity.Endpoints.RefreshToken;

internal static class Endpoint
{
    internal static async Task<Ok<TokenResponse>> HandleAsync(
        [FromServices] IUserService userService,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var refreshToken = context.Request.Cookies[identityOptions.Value.RefreshTokenCookieName];
        
        var response = await userService.RefreshTokenAsync(refreshToken, context.IpAddress(), cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            context.SetRefreshTokenCookie(response.RefreshToken, identityOptions.Value);
        }
        
        return TypedResults.Ok(response);
    }
}