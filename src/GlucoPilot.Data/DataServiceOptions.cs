using System;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Data;

public class DataServiceOptions
{
    [Required] public TimeSpan RunInterval { get; set; }
    [Required] public int DataAggregationAge { get; set; } = 30;
    [Required] public int DataExpireAge { get; set; } = 90;
}