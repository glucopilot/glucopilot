using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Identity.Extensions;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Identity.Endpoints.RevokeToken;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, ValidationProblem, UnauthorizedHttpResult>> HandleAsync(
        [FromBody] RevokeTokenRequest request,
        [FromServices] IValidator<RevokeTokenRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IUserService userService,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var token = new RevokeTokenRequest
        { Token = request.Token ?? httpContext.Request.Cookies[identityOptions.Value.RefreshTokenCookieName] };

        if (await validator.ValidateAsync(token, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();
        var user = await userService.FindByRefreshTokenAsync(token.Token, cancellationToken).ConfigureAwait(false);
        if (userId != user.Id || string.IsNullOrWhiteSpace(token.Token))
        {
            throw new UnauthorizedException("UNAUTHORIZED");
        }

        await userService.RevokeTokenAsync(token.Token, httpContext.IpAddress(), cancellationToken).ConfigureAwait(false);
        return TypedResults.NoContent();
    }
}