using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace GlucoPilot.Identity.Authentication;

public interface ICurrentUser
{
    Guid? GetUserId();

    string? GetUserEmail();
    
    bool IsAuthenticated();
    
    IEnumerable<Claim> GetClaimsIdentity();
}