using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Identity.Extensions;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Identity.Endpoints.Login;

internal static class Endpoint
{
    internal static async Task<Results<Ok<LoginResponse>, ValidationProblem>> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] IUserService userService,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var response = await userService.LoginAsync(request, httpContext.IpAddress(), cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            httpContext.SetRefreshTokenCookie(response.RefreshToken, identityOptions.Value);
        }
        return TypedResults.Ok(response);
    }
}