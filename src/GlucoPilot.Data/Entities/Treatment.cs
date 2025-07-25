using GlucoPilot.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// A treatment represents a medical intervention related to diabetes management.
/// This can be eating a meal, a correction of insulin or carbs or an injection.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("treatments")]
public class Treatment
{
    /// <summary>
    /// Unique identifier for the treatment.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who created the treatment.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user who created the treatment.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// The date and time when the treatment was created.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The date and time when the treatment was last updated.
    /// </summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// The type of treatment, this can be a meal, an injection or a correction.
    /// </summary>
    [NotMapped]
    public TreatmentType Type
    {
        get
        {
            if (Meals.Count > 0 && InjectionId is null)
            {
                return TreatmentType.Correction;
            }

            if (Meals.Count > 0 && InjectionId is not null)
            {
                return TreatmentType.Meal;
            }

            if (Meals.Count == 0 && InjectionId is not null)
            {
                return TreatmentType.Injection;
            }

            throw new InvalidOperationException("Invalid treatment type");
        }
    }

    /// <summary>
    /// The ratio of insulin to carbs for the meal associated with this treatment.
    /// </summary>
    [NotMapped]
    public decimal? InsulinToCarbRatio
    {
        get
        {
            var mealIngredients = Meals.Where(m => m.Meal != null)
                .SelectMany(m => m.Meal!.MealIngredients
                    .Where(mi => mi.Ingredient is not null)
                    .Select(mi => mi.Ingredient));

            if ((Ingredients.Count == 0 && !mealIngredients.Any()) || Injection is null || Injection.Units.Equals(0))
            {
                return null;
            }

            var ingredientCarbs = Ingredients
                .Where(ti => ti.Ingredient != null)
                .Sum(ti => ti.Ingredient!.Carbs * ti.Quantity);

            var mealIngredientsCarbs = Meals
                .Where(tm => tm.Meal != null)
                .Sum(tm => tm.Meal!.MealIngredients
                .Where(mi => mi.Ingredient != null)
                .Sum(mi => mi.Ingredient!.Carbs * mi.Quantity * tm.Quantity));

            var carbs = ingredientCarbs + mealIngredientsCarbs;
            var insulin = (decimal)Injection.Units;

            return carbs / insulin;
        }
    }

    /// <summary>
    /// The Id of the last reading (within a time frame) that this treatment is associated with.
    /// </summary>
    public Guid? ReadingId { get; set; }

    /// <summary>
    /// The last reading (within a time frame) associated with this treatment.
    /// </summary>
    public virtual Reading? Reading { get; set; }

    /// <summary>
    /// The Id of the meal that this treatment is associated with.
    /// </summary>
    [NotMapped]
    public Guid? MealId { get; set; }

    /// <summary>
    /// The meal associated with this treatment.
    /// </summary>
    [NotMapped]
    public virtual Meal? Meal { get; set; }

    /// <summary>
    /// The meals associated with this treatment.
    /// </summary>
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ICollection<TreatmentMeal> Meals { get; set; } = [];

    /// <summary>
    /// The ingredients associated with this treatment.
    /// </summary>
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ICollection<TreatmentIngredient> Ingredients { get; set; } = [];

    /// <summary>
    /// The Id of the injection that this treatment is associated with.
    /// </summary>
    public Guid? InjectionId { get; set; }

    /// <summary>
    /// The injection associated with this treatment.
    /// </summary>
    public virtual Injection? Injection { get; set; }
}
