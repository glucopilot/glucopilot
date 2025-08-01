﻿using GlucoPilot.Api.Models;
using System;
using System.Collections.Generic;
using TreatmentType = GlucoPilot.Api.Models.TreatmentType;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments;

public record ListTreatmentsResponse : PagedResponse
{
    public required ICollection<ListTreatmentResponse> Treatments { get; init; } = [];
}

public record ListTreatmentResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public required TreatmentType Type { get; init; }
    public ICollection<ListTreatmentMealResponse> Meals { get; init; } = [];
    public ICollection<ListTreatmentIngredientResponse> Ingredients { get; init; } = [];
    public ListTreatmentInjectionResponse? Injection { get; init; }
    public ListTreatmentReadingResponse? Reading { get; init; }
}

public record ListTreatmentMealResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal TotalCarbs { get; init; }
    public required decimal TotalProtein { get; init; }
    public required decimal TotalFat { get; init; }
    public required decimal TotalCalories { get; init; }
    public required decimal Quantity { get; init; }
}

public record ListTreatmentIngredientResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Quantity { get; init; }
    public required UnitOfMeasurement Uom { get; init; }
    public required decimal Carbs { get; init; }
    public required decimal Protein { get; init; }
    public required decimal Fat { get; init; }
    public required decimal Calories { get; init; }
}

public record ListTreatmentInjectionResponse
{
    public required Guid Id { get; init; }
    public required string InsulinName { get; init; }
    public required double Units { get; init; }
    public required DateTimeOffset Created { get; init; }
}

public record ListTreatmentReadingResponse
{
    public required Guid Id { get; init; }
    public required double GlucoseLevel { get; init; }
}