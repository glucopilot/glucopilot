using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;
using GlucoPilot.Data.Enums;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class UpdateIngredientValidatorTests
{
    private UpdateIngredientRequest.UpdateIngredientRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateIngredientRequest.UpdateIngredientRequestValidator();
    }

    [Test]
    public void Should_Have_Error_For_Invalid_Request()
    {
        var model = new UpdateIngredientRequest
        {
            Name = "",
            Carbs = -1,
            Protein = -1,
            Fat = -1,
            Calories = -1,
            Uom = UnitOfMeasurement.Grams
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Protein);
        result.ShouldHaveValidationErrorFor(x => x.Carbs);
        result.ShouldHaveValidationErrorFor(x => x.Fat);
        result.ShouldHaveValidationErrorFor(x => x.Calories);
    }

    [Test]
    public void Should_Not_Have_Error_For_Valid_Request()
    {
        var model = new UpdateIngredientRequest
        {
            Name = "Valid Ingredient",
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}