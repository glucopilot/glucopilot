using FluentValidation;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;

public record NewIngredientRequest
{
    public required string Name { get; set; }
    public int Carbs { get; set; }
    public int Protein { get; set; }
    public int Fat { get; set; }
    public int Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }

    public sealed class NewIngredientRequestValidator : AbstractValidator<NewIngredientRequest>
    {
        public NewIngredientRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("NAME_IS_REQUIRED");
            RuleFor(x => x.Carbs)
                .GreaterThan(0)
                .WithMessage("CARBS_GEATER_THAN_ZERO");
            RuleFor(x => x.Protein)
                .GreaterThan(0)
                .WithMessage("PROTEIN_GEATER_THAN_ZERO");
            RuleFor(x => x.Fat)
                .GreaterThan(0)
                .WithMessage("FAT_GEATER_THAN_ZERO");
            RuleFor(x => x.Calories)
                .GreaterThan(0)
                .WithMessage("CALORIES_GEATER_THAN_ZERO");
        }
    }
}
