using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlucoPilot.Identity.Authentication;

internal sealed class CurrentUserMiddleware : IMiddleware
{
    private readonly ICurrentUserInitializer _currentUserInitializer;

    public CurrentUserMiddleware(ICurrentUserInitializer currentUserInitializer)
    {
        _currentUserInitializer =
            currentUserInitializer ?? throw new ArgumentNullException(nameof(currentUserInitializer));
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _currentUserInitializer.SetCurrentUser(context.User);

        return next(context);
    }
}