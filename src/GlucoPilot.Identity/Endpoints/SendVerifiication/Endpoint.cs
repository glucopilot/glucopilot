using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Identity.Endpoints.SendVerifiication;

internal static class Endpoint
{
    internal static async Task<NoContent> HandleAsync(
        [FromQuery] string email,
        [FromServices] IRepository<User> userRepository,
        [FromServices] IUserService userService,
        CancellationToken cancellationToken)
    {

        var user = await userRepository.FindOneAsync(x => x.Email == email, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (user is not null)
        {
            await userService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, cancellationToken)
                .ConfigureAwait(false);
        }
        
        // Always return 204 No Content to prevent email enumeration
        return TypedResults.NoContent();
    }
}