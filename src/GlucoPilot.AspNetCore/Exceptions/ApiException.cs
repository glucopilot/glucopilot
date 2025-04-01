using System;
using System.Collections.Generic;
using System.Net;

namespace GlucoPilot.AspNetCore.Exceptions;

public class ApiException : Exception
{
    public List<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }

    public ApiException(string message, List<string>? errors = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        ErrorMessages = errors;
        StatusCode = statusCode;
    }
}