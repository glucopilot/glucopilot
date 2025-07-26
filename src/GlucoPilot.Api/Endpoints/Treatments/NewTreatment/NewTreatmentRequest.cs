using FluentValidation;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentRequest
{
    public DateTimeOffset? Created { get; set; }
    public ICollection<NewTreatmentMeal> Meals { get; set; } = [];
    public ICollection<NewTreatmentIngredient> Ingredients { get; set; } = [];
    public NewInjection? Injection { get; set; }
    public Guid? ReadingId { get; set; }

    public class NewTreatmentRequestValidator : AbstractValidator<NewTreatmentRequest>
    {
        public NewTreatmentRequestValidator()
        {
            RuleFor(request => request)
                .Must(request => request.Meals.Count > 0 || request.Ingredients.Count > 0 || request.Injection != null)
                .WithMessage(Resources.ValidationMessages.MealInjectionIdInjectionIdMustBeProvided);
            RuleFor(x => x.Injection.InsulinId)
                .NotEmpty()
                .When(request => request.Injection is not null)
                .WithMessage(Resources.ValidationMessages.InsulinIdInvalid);
            RuleFor(x => x.Injection.Units)
                .GreaterThan(0)
                .When(request => request.Injection is not null)
                .WithMessage(Resources.ValidationMessages.UnitsGreaterThanZero);
            RuleFor(x => x.Injection.Created)
                .NotEmpty()
                .When(request => request.Injection is not null)
                .WithMessage(Resources.ValidationMessages.CreatedDateRequired);
        }
    }
}

public record NewInjection
{
    public Guid InsulinId { get; set; }
    public double Units { get; set; }
    public DateTimeOffset Created { get; set; }
}

public record NewTreatmentMeal
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
}

public record NewTreatmentIngredient
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
}