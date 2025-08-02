using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Nutrition;

[ExcludeFromCodeCoverage]
public sealed class NutritionOptions
{
    [Required] public string ConnectionString { get; init; } = "";
}