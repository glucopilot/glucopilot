using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insights.Insulin;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class InsulinInsightsRequestValidatorTests
{
    private readonly InsulinInsightsRequest.Validator _validator;
    
    public InsulinInsightsRequestValidatorTests()
    {
        _validator = new InsulinInsightsRequest.Validator();
    }
    
    [Test]
    public void Should_Have_Error_When_StartDate_Is_Greater_Than_EndDate()
    {
        var model = new InsulinInsightsRequest { From = DateTime.UtcNow.AddDays(1), To = DateTime.UtcNow };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }
}