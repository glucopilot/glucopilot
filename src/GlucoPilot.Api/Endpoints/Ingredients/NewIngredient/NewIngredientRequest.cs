using FluentValidation;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;

public record NewIngredientRequest
{
    public required string Name { get; set; }
    public decimal Carbs { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }

    public sealed class NewIngredientRequestValidator : AbstractValidator<NewIngredientRequest>
    {
        public NewIngredientRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(Resources.ValidationMessages.NameRequired);
            RuleFor(x => x.Carbs)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.CarbsGreaterThanOrEqualToZero);
            RuleFor(x => x.Protein)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.ProteinGreaterThanOrEqualToZero);
            RuleFor(x => x.Fat)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.FatGreaterThanOrEqualToZero);
            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.CaloriesGreaterThanOrEqualToZero);
        }
    }
}
