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
                When(x => x.InjectionId is null && x.ReadingId is null && x.MealId is null, () =>
                {
                    RuleFor(x => x.InjectionId)
                        .NotNull()
                        .WithMessage("InjectionId cannot be null if all other properties are null.");

                    RuleFor(x => x.ReadingId)
                        .NotNull()
                        .WithMessage("ReadingId cannot be null if all other properties are null.");

                    RuleFor(x => x.MealId)
                        .NotNull()
                        .WithMessage("MealId cannot be null if all other properties are null.");
                });
            }
        }
    }
}