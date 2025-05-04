using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insights.InsulinToCarbRatio;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class InsulinToCarbRatioRequestValidatorTests
{
    private readonly InsulinToCarbRatioRequest.Validator _validator;

    public InsulinToCarbRatioRequestValidatorTests()
    {
        _validator = new InsulinToCarbRatioRequest.Validator();
    }

    [Test]
    public void Should_Have_Error_When_StartDate_Is_Greater_Than_EndDate()
    {
        var model = new InsulinToCarbRatioRequest { From = DateTime.UtcNow.AddDays(1), To = DateTime.UtcNow };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }
}