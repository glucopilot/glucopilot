using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Insights.InsulinOnBoard;

public record InsulinOnBoardResponse
{
    public ICollection<InsulinOnBoardTreatmentResponse> Treatments { get; set; } = [];
}

public record InsulinOnBoardTreatmentResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public InsulinOnBoardInjectionResponse Injection { get; set; }
}

public record InsulinOnBoardInjectionResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public double Units { get; set; }
    public InsulinOnBoardInsulinResponse Insulin { get; set; }
}
public record InsulinOnBoardInsulinResponse
{
    public Guid Id { get; set; }
    public double Duration { get; set; }
    public double PeakTime { get; set; }
    public double Scale { get; set; }
}
