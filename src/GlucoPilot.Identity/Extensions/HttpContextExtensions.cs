using System;
using Microsoft.AspNetCore.Http;

namespace GlucoPilot.Identity.Extensions;

internal static class HttpContextExtensions
{
    internal static string IpAddress(this HttpContext context)
    {
        return context.Request.Headers.TryGetValue("X-Forwarded-For", out var value)
            ? value.ToString()
            : context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }

    internal static void SetRefreshTokenCookie(this HttpContext context, string refreshToken, IdentityOptions options)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
#if DEBUG
            Secure = false,
#else
            Secure = true,
#endif
            Expires = DateTimeOffset.UtcNow.AddDays(options.RefreshTokenExpirationInDays),
        };

        context.Response.Cookies.Append(
            options.RefreshTokenCookieName,
            refreshToken,
            cookieOptions);
    }
}