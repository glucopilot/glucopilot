using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;
using NUnit.Framework;
using System;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class UpdateTreatmentRequestValidatorTests
{
    private UpdateTreatmentRequest.UpdateTreatmentRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateTreatmentRequest.UpdateTreatmentRequestValidator();
    }

    [Test]
    public void Validate_Should_Return_Error_When_All_Ids_Are_Null()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            MealId = null,
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
            x => x.ErrorMessage == "InjectionId cannot be null if all other properties are null."));
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
                x => x.ErrorMessage == "MealId cannot be null if all other properties are null."));
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
                x => x.ErrorMessage == "ReadingId cannot be null if all other properties are null."));
        });        
    }

    [Test]
    public void Validate_Should_Pass_When_Only_InjectionId_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = Guid.NewGuid(),
            MealId = null,
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Only_MealId_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            MealId = Guid.NewGuid(),
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Only_ReadingId_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            MealId = null,
            ReadingId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Multiple_Ids_Are_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = Guid.NewGuid(),
            MealId = Guid.NewGuid(),
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }
}