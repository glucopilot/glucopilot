using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Identity.Endpoints.IsVerified;

internal static class Endpoint
{
    internal static async Task<Results<NoContent, ForbidHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] IsVerifiedRequest request,
        [FromServices] IValidator<IsVerifiedRequest> validator,
        [FromServices] IRepository<User> userRepository,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        if (!identityOptions.Value.RequireEmailVerification)
        {
            return TypedResults.NoContent();
        }

        var user = userRepository.FindOne(u => u.Email == request.Email);
        if (user is null)
        {
            throw new ForbiddenException("USER_NOT_VERIFIED");
        }

        if (user.IsVerified)
        {
            return TypedResults.NoContent();
        }
        throw new ForbiddenException("USER_NOT_VERIFIED");
    }
}