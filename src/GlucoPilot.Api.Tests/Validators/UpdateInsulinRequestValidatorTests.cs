using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insulins.UpdateInsulin;
using GlucoPilot.Data.Enums;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
class UpdateInsulinRequestValidatorTests
{
    private UpdateInsulinRequest.UpdateInsulinRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateInsulinRequest.UpdateInsulinRequestValidator();
    }

    [Test]
    public void Validator_Should_Have_Error_When_Name_Is_Empty()
    {
        var request = new UpdateInsulinRequest { Name = "", Type = InsulinType.Bolus };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validator_Should_Have_Error_When_Type_Is_Invalid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = (InsulinType)(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Type_Is_Valid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void Validator_Should_Have_Error_When_Duration_Is_Less_Than_Or_Equal_To_Zero()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, Duration = -1 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Duration_Is_Valid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, Duration = 5 };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Duration);
    }

    [Test]
    public void Validator_Should_Have_Error_When_Scale_Is_Less_Than_Or_Equal_To_Zero()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, Scale = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Scale);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Scale_Is_Valid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, Scale = 1.5 };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Scale);
    }

    [Test]
    public void Validator_Should_Have_Error_When_PeakTime_Is_Less_Than_Or_Equal_To_Zero()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, PeakTime = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PeakTime);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_PeakTime_Is_Valid()
    {
        var request = new UpdateInsulinRequest { Name = "Valid Name", Type = InsulinType.Bolus, PeakTime = 2 };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PeakTime);
    }
}
