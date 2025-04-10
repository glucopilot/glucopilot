using System;
using System.Collections.Generic;
using System.Security.Claims;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Identity.Extensions;

namespace GlucoPilot.Identity.Authentication;

internal sealed class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? _user = null;

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    /// <returns>The current user's ID</returns>
    /// <exception cref="UnauthorizedException">When called from a place the user should not be authenticated.</exception>
    public Guid GetUserId() => _user?.GetUserId() ?? throw new UnauthorizedException("USER_NOT_LOGGED_IN");

    public string? GetUserEmail() => _user?.GetEmail() ?? null;

    public bool IsAuthenticated() => _user?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<Claim> GetClaimsIdentity() => _user?.Claims ?? Array.Empty<Claim>();

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (_user is not null)
        {
            throw new InvalidOperationException("Method reserved for in-scope initialization.");
        }

        _user = user;
    }
}