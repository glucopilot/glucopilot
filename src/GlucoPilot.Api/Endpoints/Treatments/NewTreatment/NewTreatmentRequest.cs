using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentRequest
{
    public DateTimeOffset? Created { get; set; }
    public Guid? MealId { get; set; }
    public Guid? InjectionId { get; set; }
    public Guid? ReadingId { get; set; }

    public class NewTreatmentRequestValidator : AbstractValidator<NewTreatmentRequest>
    {
        public NewTreatmentRequestValidator()
        {
            RuleFor(request => request)
                .Must(request => request.MealId != null || request.InjectionId != null)
                .WithMessage("At least one of MealId, InjectionId must be provided.");
        }
    }
}