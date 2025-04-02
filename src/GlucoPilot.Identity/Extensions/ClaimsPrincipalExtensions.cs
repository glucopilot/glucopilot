using System;
using System.Security.Claims;

namespace GlucoPilot.Identity.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal?.FindFirstValue(ClaimTypes.Email);

    private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
        principal is null
            ? throw new ArgumentNullException(nameof(principal))
            : principal?.FindFirst(claimType)?.Value;
}