using FluentValidation;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;

public sealed record UpdateIngredientRequest
{
    public required string Name { get; set; }
    public required int Carbs { get; set; }
    public required int Protein { get; set; }
    public required int Fat { get; set; }
    public required int Calories { get; set; }
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
                .WithMessage(Resources.ValidationMessages.CarbsGreaterThanZero);
            RuleFor(x => x.Protein)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.ProteinGreaterThanZero);
            RuleFor(x => x.Fat)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.FatGreaterThanZero);
            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Resources.ValidationMessages.CaloriesGreaterThanZero);
        }
    }
}
