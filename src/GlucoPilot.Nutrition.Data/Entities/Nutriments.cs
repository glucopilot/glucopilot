using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Nutrition.Data.Entities;

[Owned]
public class Nutriments
{
    public string? EnergyUnit { get; set; }

    public string? FatUnit { get; set; }

    public string? CarbohydratesUnit { get; set; }

    public string? EnergyKcalUnit { get; set; }

    public double? EnergyKcalValue { get; set; }

    public double? EnergyValue { get; set; }

    public double? CarbohydratesValue { get; set; }

    public float? Proteins { get; set; }

    public double? EnergyKcalValueComputed { get; set; }

    public float? ProteinsValue { get; set; }

    public double? EnergyKcal { get; set; }

    public string? ProteinsUnit { get; set; }

    public double? Carbohydrates { get; set; }

    public float? Energy { get; set; }

    public float? Fat { get; set; }

    public float? FatValue { get; set; }
}