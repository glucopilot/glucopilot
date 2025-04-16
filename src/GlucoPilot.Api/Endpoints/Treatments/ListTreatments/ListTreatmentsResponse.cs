using GlucoPilot.Api.Models;
using GlucoPilot.Data.Enums;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments
{
    public record ListTreatmentsResponse : PagedResponse
    {
        public required ICollection<ListTreatmentResponse> Treatments { get; init; } = Array.Empty<ListTreatmentResponse>();
    }

    public record ListTreatmentResponse
    {
        public required Guid Id { get; init; }
        public required DateTimeOffset Created { get; init; }
        public required TreatmentType Type { get; init; }
        public ListTreatmentMealResponse? Meal { get; init; }
        public ListTreatmentInjectionResponse? Injection { get; init; }
        public ListTreatmentReadingResponse? Reading { get; init; }
    }

    public record ListTreatmentMealResponse
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required int TotalCarbs { get; init; }
        public required int TotalProtein { get; init; }
        public required int TotalFat { get; init; }
        public required int TotalCalories { get; init; }
    }

    public record ListTreatmentInjectionResponse
    {
        public required Guid Id { get; init; }
        public required string InsulinName { get; init; }
        public required double Units { get; init; }
    }

    public record ListTreatmentReadingResponse
    {
        public required Guid Id { get; init; }
        public required double GlucoseLevel { get; init; }
    }
}