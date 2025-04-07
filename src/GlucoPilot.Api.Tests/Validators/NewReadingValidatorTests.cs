using FluentValidation.TestHelper;
using GlucoPilot.Api.Models;
using NUnit.Framework;
using System;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
class NewReadingValidatorTests
{
    private NewReadingValidator _validator;

    [Test]
    public void Should_Have_Error_When_Created_Is_In_Future()
    {
        var model = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow.AddDays(1),
            GlucoseLevel = 5.0
        };

        _validator = new NewReadingValidator();
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Created);
    }

    [Test]
    public void Should_Have_Error_When_GlucoseLevel_Is_Zero()
    {
        var model = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 0.0
        };
        _validator = new NewReadingValidator();
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.GlucoseLevel);
    }

    [Test]
    public void Should_Have_Error_When_GlucoseLevel_Is_Negative()
    {
        var model = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = -1.0
        };
        _validator = new NewReadingValidator();
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.GlucoseLevel);
    }

    [Test]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var model = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 5.0
        };
        _validator = new NewReadingValidator();
        var result = _validator.TestValidate(model);
        Assert.Multiple(() =>
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Created);
            result.ShouldNotHaveValidationErrorFor(x => x.GlucoseLevel);
        });
    }
}
