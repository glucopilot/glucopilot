using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public sealed class ForbiddenException : ApiException
{
    public ForbiddenException(string message) 
        : base(message, null, HttpStatusCode.Forbidden)
    {
    }
}