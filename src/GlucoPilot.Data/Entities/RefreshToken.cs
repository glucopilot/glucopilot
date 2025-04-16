using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Entities;

[Owned]
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Token { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Expires { get; set; }

    public required string CreatedByIp { get; set; }

    public DateTimeOffset? Revoked { get; set; }

    public string? RevokedByIp { get; set; }

    public string? RevokedReason { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= Expires;

    public bool IsRevoked => Revoked is not null;

    public bool IsActive => !IsExpired && !IsRevoked;
}