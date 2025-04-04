using GlucoPilot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data;

public class GlucoPilotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealIngredient> MealIngredients { get; set; }


    public GlucoPilotDbContext(DbContextOptions<GlucoPilotDbContext> options) : base(options)
    {
    }
}