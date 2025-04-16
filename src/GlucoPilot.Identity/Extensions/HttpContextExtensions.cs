using System;
using Microsoft.AspNetCore.Http;

namespace GlucoPilot.Identity.Extensions;

internal static class HttpContextExtensions
{
    internal static string IpAddress(this HttpContext context)
    {
        return context.Request.Headers.ContainsKey("X-Forwarded-For")
            ? context.Request.Headers["X-Forwarded-For"].ToString()
            : context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }

    internal static void SetRefreshTokenCookie(this HttpContext context, string refreshToken, IdentityOptions options)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(options.RefreshTokenExpirationInDays),
        };

        context.Response.Cookies.Append(
            options.RefreshTokenCookieName,
            refreshToken,
            cookieOptions);
    }
}