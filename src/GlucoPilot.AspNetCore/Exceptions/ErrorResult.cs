using System;
using System.Collections.Generic;

namespace GlucoPilot.AspNetCore.Exceptions;

public sealed class ErrorResult
{
    public string? Source { get; init; }
    public required string Message { get; init; }
    public required Guid ErrorId { get; set; }
    public required string SupportMessage { get; init; }
    public required int StatusCode { get; init; }
    public List<string> Messages { get; init; } = [];
}