using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
[Table("users")]
public abstract class User
{
    /// <summary>
    /// The unique identifier for the user.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The date and time the user was created.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The date and time the user was last updated.
    /// </summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// The users email address.
    /// </summary>
    [MaxLength(320)]
    public required string Email { get; set; }

    /// <summary>
    /// Hashed password.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Whether the user has accepted the terms of service.
    /// </summary>
    public bool AcceptedTerms { get; set; }

    /// <summary>
    /// Token used for email verification.
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// Whether the user has verified their email address.
    /// </summary>
    public bool IsVerified { get; set; }
    
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>(); 

    /// <summary>
    /// A collection of patients that this user has access to.
    /// </summary>
    public virtual ICollection<Patient> Patients { get; set; } = [];
}