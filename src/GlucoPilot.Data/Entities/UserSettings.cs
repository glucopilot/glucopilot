using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GlucoPilot.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Entities;

[Table("user_settings")]
[Owned]
public class UserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid UserId { get; set; }
    
    public virtual User? User { get; set; }

    public GlucoseUnitOfMeasurement GlucoseUnitOfMeasurement { get; set; } = GlucoseUnitOfMeasurement.MmolL;

    public double LowSugarThreshold { get; set; } = 4;

    public double HighSugarThreshold { get; set; } = 10;

    public int DailyCalorieTarget { get; set; } = 2000;

    public int DailyCarbTarget { get; set; } = 300;

    public int DailyProteinTarget { get; set; } = 150;

    public int DailyFatTarget { get; set; } = 120;
}