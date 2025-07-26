using FluentValidation;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;

public sealed record UpdateTreatmentRequest
{
    public Guid? InjectionId { get; set; }
    public ICollection<UpdateTreatmentMealRequest> Meals { get; set; } = [];
    public ICollection<UpdateTreatmentIngredientRequest> Ingredients { get; set; } = [];
    public Guid? ReadingId { get; set; }

    public sealed class UpdateTreatmentRequestValidator : AbstractValidator<UpdateTreatmentRequest>
    {
        public UpdateTreatmentRequestValidator()
        {
            When(x => x.InjectionId is null && x.ReadingId is null && x.Meals.Count == 0 && x.Ingredients.Count == 0, () =>
            {
                RuleFor(x => x.InjectionId)
                    .NotNull()
                    .WithMessage(Resources.ValidationMessages.InjectionIdRequiredWhenAllNull);

                RuleFor(x => x.ReadingId)
                    .NotNull()
                    .WithMessage(Resources.ValidationMessages.ReadingIdRequiredWhenAllNull);

                RuleFor(x => x.Meals)
                    .NotEmpty()
                    .WithMessage(Resources.ValidationMessages.MealRequiredWhenAllNull);

                RuleFor(x => x.Ingredients)
                    .NotEmpty()
                    .WithMessage(Resources.ValidationMessages.IngredientRequiredWhenAllNull);
            });
        }
    }

    public record UpdateTreatmentMealRequest
    {
        public Guid Id { get; set; }
        public decimal Quantity { get; set; }
    }

    public record UpdateTreatmentIngredientRequest
    {
        public Guid Id { get; set; }
        public decimal Quantity { get; set; }
    }
}