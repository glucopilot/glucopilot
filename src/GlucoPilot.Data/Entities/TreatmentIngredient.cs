using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Data.Entities
{
    [ExcludeFromCodeCoverage]
    [Table("treatments_ingredients")]
    public class TreatmentIngredient
    {
        /// <summary>
        /// The unique identifier for the treatment ingredient.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// The unique identifier for the treatment.
        /// </summary>
        public required Guid TreatmentId { get; set; }

        /// <summary>
        ///  The treatment the treatment ingredient is associated with.
        /// </summary>
        public virtual Treatment? Treatment { get; set; }

        /// <summary>
        /// The unique identifier for the ingredient.
        /// </summary>
        public required Guid IngredientId { get; set; }

        /// <summary>
        /// The ingredient the treatment ingredient is associated with.
        /// </summary>
        public virtual Ingredient? Ingredient { get; set; }

        /// <summary>
        /// The quantity of the ingredient in the treatment.
        /// </summary>
        [Range(0, int.MaxValue)]
        public required decimal Quantity { get; set; }
    }
}
