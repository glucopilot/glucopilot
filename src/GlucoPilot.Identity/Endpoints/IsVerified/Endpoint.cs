using System.Collections.Generic;
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

namespace GlucoPilot.Identity.Endpoints.IsVerified;

internal static class Endpoint
{
    internal static async Task<Results<Ok, ForbidHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] IsVerifiedRequest request,
        [FromServices] IValidator<IsVerifiedRequest> validator,
        [FromServices] IRepository<User> userRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }
        
        var user = userRepository.FindOne(u => u.Email == request.Email);
        if (user is null)
        {
            throw new ForbiddenException("USER_NOT_VERIFIED");
        }
        
        if (user.IsVerified)
        {
            return TypedResults.Ok();
        }
        throw new ForbiddenException("USER_NOT_VERIFIED");
    }
}