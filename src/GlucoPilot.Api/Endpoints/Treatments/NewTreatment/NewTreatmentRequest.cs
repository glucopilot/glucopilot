using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentRequest
{
    public DateTimeOffset? Created { get; set; }
    public Guid? MealId { get; set; }
    public NewInjection? Injection { get; set; }
    public Guid? ReadingId { get; set; }

    public class NewTreatmentRequestValidator : AbstractValidator<NewTreatmentRequest>
    {
        public NewTreatmentRequestValidator()
        {
            RuleFor(request => request)
                .Must(request => request.MealId != null || request.Injection != null)
                .WithMessage(Resources.ValidationMessages.MealdInjectionIdMustBeProvided);
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