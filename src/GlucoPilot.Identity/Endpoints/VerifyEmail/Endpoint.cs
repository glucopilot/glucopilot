using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Identity.Endpoints.VerifyEmail;

internal static class Endpoint
{
    internal static async Task<Results<ContentHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] VerifyEmailRequest request,
        [FromServices] IValidator<VerifyEmailRequest> validator,
        [FromServices] IUserService userService,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        await userService.VerifyEmailAsync(request, cancellationToken).ConfigureAwait(false);
        return TypedResults.Content("<h1>Success</h1><p>Your email has been verified. You may now close this page.</p>",
            "text/html", Encoding.UTF8, statusCode: StatusCodes.Status200OK);
    }
}