using System;
using System.Collections.Generic;
using System.Security.Claims;
using GlucoPilot.Identity.Extensions;

namespace GlucoPilot.Identity.Authentication;

internal sealed class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? _user = null;

    public Guid? GetUserId() => _user?.GetUserId() ?? null;

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