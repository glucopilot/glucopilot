using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GlucoPilot.Api.Middleware;

internal partial class ExceptionMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ICurrentUser? _currentUser;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ICurrentUser? currentUser, ILogger<ExceptionMiddleware> logger)
    {
        _currentUser = currentUser;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var errorId = Guid.NewGuid();
            var logContext = new Dictionary<string, object?>()
            {
                { "Email", _currentUser?.GetUserEmail() },
                { "ErrorId", errorId },
            };
            if (context.User.Identity?.IsAuthenticated == true)
            {
                logContext.Add("UserId", _currentUser?.GetUserId());
            }
            
            using var scope = _logger.BeginScope(logContext);

            context.Response.ContentType = "application/json";
            if (e is not ApiException && e.InnerException is not null)
            {
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                }
            }

            var messages = new List<string>();
            switch (e)
            {
                case ConflictException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    messages.Add(e.Message);
                    break;
                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    messages.Add(e.Message);
                    break;
                case UnauthorizedException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    messages.Add(e.Message);
                    break;
                case ForbiddenException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    messages.Add(e.Message);
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var result = new ErrorResult
            {
                Source = e.TargetSite?.DeclaringType?.FullName,
                Message = e.Message,
                ErrorId = errorId,
                SupportMessage = "SUPPORT_MESSAGE",
                StatusCode = context.Response.StatusCode,
                Messages = messages,
            };
            LogException(e);
            await JsonSerializer
                .SerializeAsync(context.Response.Body, result, JsonSerializerOptions, context.RequestAborted)
                .ConfigureAwait(false);
        }
    }

    [LoggerMessage(LogLevel.Error, "Request exception")]
    private partial void LogException(Exception error);
}