using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public class NotFoundException(string message) : ApiException(message, null, HttpStatusCode.NotFound);