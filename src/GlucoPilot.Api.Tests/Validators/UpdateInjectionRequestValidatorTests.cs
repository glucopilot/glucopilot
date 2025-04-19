using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Injections.UpdateInjection;
using GlucoPilot.Data.Enums;
using NUnit.Framework;
using System;

[TestFixture]
public class UpdateInjectionRequestValidatorTests
{
    private UpdateInjectionRequest.UpdateInjectionRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateInjectionRequest.UpdateInjectionRequestValidator();
    }

    [Test]
    public void Validate_Returns_Error_When_InsulinId_Is_Empty()
    {
        var request = new UpdateInjectionRequest
        {
            InsulinId = Guid.Empty,
            Units = 10,
            Type = InsulinType.Bolus
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.InsulinId).WithErrorMessage("INSULIN_ID_INVALID");
    }

    [Test]
    public void Validate_Returns_Error_When_Units_Are_Less_Than_Or_Equal_To_Zero()
    {
        var request = new UpdateInjectionRequest
        {
            InsulinId = Guid.NewGuid(),
            Units = 0,
            Type = InsulinType.Bolus
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Units).WithErrorMessage("UNITS_GREATER_THAN_ZERO");
    }

    [Test]
    public void Validate_Returns_Error_When_Type_Is_Invalid()
    {
        var request = new UpdateInjectionRequest
        {
            InsulinId = Guid.NewGuid(),
            Units = 10,
            Type = (InsulinType)(-1)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Type).WithErrorMessage("INSULIN_TYPE_INVALID");
    }

    [Test]
    public void Validate_Passes_When_All_Fields_Are_Valid()
    {
        var request = new UpdateInjectionRequest
        {
            InsulinId = Guid.NewGuid(),
            Units = 10,
            Type = InsulinType.Bolus
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }
}