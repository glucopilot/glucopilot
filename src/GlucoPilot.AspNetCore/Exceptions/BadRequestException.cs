using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public class BadRequestException(string message) : ApiException(message, null, HttpStatusCode.BadRequest)
{
}
