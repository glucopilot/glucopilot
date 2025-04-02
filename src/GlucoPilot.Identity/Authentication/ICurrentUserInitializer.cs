using System.Security.Claims;

namespace GlucoPilot.Identity.Authentication;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);
}