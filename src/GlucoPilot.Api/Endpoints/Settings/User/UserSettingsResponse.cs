using System;

namespace GlucoPilot.Api.Endpoints.Settings.User;

public enum GlucoseUnitOfMeasurement
{
    MmolL,
    MgDl
}

public sealed class UserSettingsResponse
{
    public GlucoseUnitOfMeasurement GlucoseUnitOfMeasurement { get; init; }
    public double LowSugarThreshold { get; init; }
    public double HighSugarThreshold { get; init; }
    public int DailyCalorieTarget { get; init; }
    public int DailyCarbTarget { get; init; }
    public int DailyProteinTarget { get; init; }
    public int DailyFatTarget { get; init; }
}