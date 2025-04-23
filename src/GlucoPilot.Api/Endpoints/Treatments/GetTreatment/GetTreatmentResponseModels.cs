﻿using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Treatments.GetTreatment;

public record GetTreatmentResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset? Updated { get; init; }
    public required TreatmentType Type { get; init; }
    public GetTreatmentMealResponse? Meal { get; init; }
    public GetTreatmentInjectionResponse? Injection { get; init; }
    public GetTreatmentReadingResponse? Reading { get; init; }
}

public record GetTreatmentMealResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal TotalCarbs { get; init; }
    public required decimal TotalProtein { get; init; }
    public required decimal TotalFat { get; init; }
    public required decimal TotalCalories { get; init; }
}

public record GetTreatmentInjectionResponse
{
    public required Guid Id { get; init; }
    public required string InsulinName { get; init; }
    public required double Units { get; init; }
}

public record GetTreatmentReadingResponse
{
    public required Guid Id { get; init; }
    public required double GlucoseLevel { get; init; }
}