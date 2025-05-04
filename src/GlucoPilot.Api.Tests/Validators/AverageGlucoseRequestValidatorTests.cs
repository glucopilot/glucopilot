using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insights.AverageGlucose;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class AverageGlucoseRequestValidatorTests
{
    private readonly AverageGlucoseRequest.Validator _validator;
    
    public AverageGlucoseRequestValidatorTests()
    {
        _validator = new AverageGlucoseRequest.Validator();
    }
    
    [Test]
    public void Should_Have_Error_When_StartDate_Is_Greater_Than_EndDate()
    {
        var model = new AverageGlucoseRequest { From = DateTime.UtcNow.AddDays(1), To = DateTime.UtcNow };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }
}