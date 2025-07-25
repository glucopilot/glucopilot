using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Nutrition;

public sealed class NutritionOptions
{
    [Required] public string ConnectionString { get; init; } = "";
}