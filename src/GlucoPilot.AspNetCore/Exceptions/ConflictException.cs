using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(message, null, HttpStatusCode.Conflict)
    {
    }
}