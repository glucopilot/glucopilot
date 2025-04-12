using FluentValidation;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;

public sealed record UpdateIngredientRequest
{
    public required Guid Id { get; set; }
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
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("ID_IS_REQUIRED");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("NAME_IS_REQUIRED");
            RuleFor(x => x.Carbs)
                .GreaterThanOrEqualTo(0)
                .WithMessage("CARBS_GREATER_THAN_ZERO");
            RuleFor(x => x.Protein)
                .GreaterThanOrEqualTo(0)
                .WithMessage("PROTEIN_GREATER_THAN_ZERO");
            RuleFor(x => x.Fat)
                .GreaterThanOrEqualTo(0)
                .WithMessage("FAT_GREATER_THAN_ZERO");
            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0)
                .WithMessage("CALORIES_GREATER_THAN_ZERO");
        }
    }
}
