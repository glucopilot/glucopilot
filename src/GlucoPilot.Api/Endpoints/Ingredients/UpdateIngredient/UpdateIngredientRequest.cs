using FluentValidation;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;

public sealed record UpdateIngredientRequest
{
    public required string Name { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }

    public sealed class UpdateIngredientRequestValidator : AbstractValidator<UpdateIngredientRequest>
    {
        public UpdateIngredientRequestValidator()
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
