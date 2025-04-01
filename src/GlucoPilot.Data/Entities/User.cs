using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Entities;

[Table("users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? Updated { get; set; }
    
    [MaxLength(320)]
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; }

    public bool AcceptedTerms { get; set; }
}