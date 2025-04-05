using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

[Table("injections")]
public class Injection
{
    /// <summary>
    /// Unique identifier for the injection.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The date and time when the injection was created.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The Id of the insulin used for the injection.
    /// </summary>
    public required Guid InsulinId { get; set; }

    /// <summary>
    /// The insulin used for the injection.
    /// </summary>
    public virtual Insulin? Insulin { get; set; }

    /// <summary>
    /// The amount of insulin injected.
    /// </summary>
    public required double Units { get; set; }
}
