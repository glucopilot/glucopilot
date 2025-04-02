using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public class UnauthorizedException(string message) : ApiException(message, null, HttpStatusCode.Unauthorized);