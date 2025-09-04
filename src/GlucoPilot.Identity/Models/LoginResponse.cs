using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GlucoPilot.Identity.Models;

public sealed record LoginResponse
{
    public required string Token { get; init; }
    public Guid UserId { get; init; }
    public required string Email { get; init; }
    public bool IsVerified { get; init; }
    public GlucoseProvider? GlucoseProvider { get; init; }
    public int? TargetLow { get; init; }
    public int? TargetHigh { get; init; }
    public required ICollection<LoginAlarmRuleResponse>? AlarmRules { get; init; }
    public string? PatientId { get; init; }

    [JsonIgnore]
    public string? RefreshToken { get; init; }
}

public sealed record LoginAlarmRuleResponse
{
    public required Guid Id { get; init; }
    public required int TargetValue { get; init; }
    public required AlarmTargetDirection TargetDirection { get; init; }
}