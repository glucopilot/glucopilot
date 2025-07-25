using System.Diagnostics.CodeAnalysis;
using GlucoPilot.Nutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Nutrition.Data;

[ExcludeFromCodeCoverage]
public class GlucoPilotNutritionDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }    

    public GlucoPilotNutritionDbContext(DbContextOptions<GlucoPilotNutritionDbContext> options) : base(options)
    {
    }
}