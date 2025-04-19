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
                        .WithMessage(Resources.ValidationMessages.InjectionIdRequiredWhenAllNull);

                    RuleFor(x => x.ReadingId)
                        .NotNull()
                        .WithMessage(Resources.ValidationMessages.ReadingIdRequiredWhenAllNull);

                    RuleFor(x => x.MealId)
                        .NotNull()
                        .WithMessage(Resources.ValidationMessages.MealIdRequiredWhenAllNull);
                });
            }
        }
    }
}