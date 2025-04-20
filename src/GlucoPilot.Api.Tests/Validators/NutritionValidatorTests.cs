using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Treatments.Nutrition;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class NutritionValidatorTests
{
    private NutritionRequestModel.Validator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new NutritionRequestModel.Validator();
    }

    [Test]
    public void Should_Have_Error_When_From_Is_Greater_Than_To()
    {
        var model = new NutritionRequestModel
        {
            From = DateTimeOffset.UtcNow.AddDays(1),
            To = DateTimeOffset.UtcNow
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }
}