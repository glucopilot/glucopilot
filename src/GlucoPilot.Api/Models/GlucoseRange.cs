namespace GlucoPilot.Api.Models;

public record GlucoseRange
{
    public required int RangeId { get; init; }
    public required double RangeMin { get; init; }
    public required double RangeMax { get; init; }
    public required int TotalMinutes { get; init; }
    public required decimal Percentage { get; init; }
};