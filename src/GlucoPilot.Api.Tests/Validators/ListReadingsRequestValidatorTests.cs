using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Readings.List;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class ListReadingsRequestValidatorTests
{
    private ListReadingValidator _validator;

    [Test]
    public void Should_Have_Error_When_From_Is_Greater_Than_To()
    {
        var model = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(1),
            To = DateTimeOffset.UtcNow
        };

        _validator = new ListReadingValidator();

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.From);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        var model = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
        };

        _validator = new ListReadingValidator();

        var result = _validator.TestValidate(model);
        Assert.Multiple(() =>
        {
            result.ShouldNotHaveValidationErrorFor(x => x.From);
            result.ShouldNotHaveValidationErrorFor(x => x.To);
        });
    }

    [Test]
    public void Should_Have_Error_When_MinuteInterval_Is_Zero()
    {
        var model = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow,
            MinuteInterval = 0
        };

        _validator = new ListReadingValidator();

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MinuteInterval);
    }

    [Test]
    public void Should_Have_Error_When_MinuteInterval_Is_Over_60()
    {
        var model = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow,
            MinuteInterval = 61
        };

        _validator = new ListReadingValidator();

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MinuteInterval);
    }
}