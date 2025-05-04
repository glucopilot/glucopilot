using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insights.AverageNutrition;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class AverageNutritionRequestValidatorTests
{
    private readonly AverageNutritionRequest.Validator _validator;

    public AverageNutritionRequestValidatorTests()
    {
        _validator = new AverageNutritionRequest.Validator();
    }

    [Test]
    public void Should_Have_Error_When_StartDate_Is_Greater_Than_EndDate()
    {
        var model = new AverageNutritionRequest { From = DateTime.UtcNow.AddDays(1), To = DateTime.UtcNow };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }

    [Test]
    public void Should_Have_Error_When_Range_Is_Less_Than_Zero()
    {
        var model = new AverageNutritionRequest { Range = TimeSpan.FromSeconds(-1) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Range);
    }
}