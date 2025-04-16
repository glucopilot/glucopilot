using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment
{
    public sealed record UpdateTreatmentRequest
    {
        public Guid? InjectionId { get; set; }
        public Guid? MealId { get; set; }
        public Guid? ReadingId { get; set; }

        public sealed class UpdateTreatmentRequestValidator : AbstractValidator<UpdateTreatmentRequest>
        {
            public UpdateTreatmentRequestValidator()
            {
                RuleFor(x => x).Must(x => x.InjectionId is not null || x.ReadingId is not null || x.MealId is not null)
                    .WithMessage("Either InjectionId, MealId or ReadingId must be provided.");
            }
        }
    }
}