namespace GlucoPilot.Data.Repository;

public sealed class FindOptions
{
    public bool IsIgnoreAutoIncludes { get; set; }
    public bool IsAsNoTracking { get; set; }
}