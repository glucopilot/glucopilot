using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Treatments.NewTreatment;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class NewTreatmentRequestValidatorTests
{
    private NewTreatmentRequest.NewTreatmentRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new NewTreatmentRequest.NewTreatmentRequestValidator();
    }

    [Test]
    public void Validator_Should_Have_Error_When_Both_MealId_And_InjectionId_Are_Null()
    {
        var request = new NewTreatmentRequest
        {
            MealId = null,
            InjectionId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.ShouldHaveValidationErrorFor(r => r), Is.Not.Null);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_MealId_Is_Provided()
    {
        var request = new NewTreatmentRequest
        {
            MealId = Guid.NewGuid(),
            InjectionId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_InjectionId_Is_Provided()
    {
        var request = new NewTreatmentRequest
        {
            MealId = null,
            InjectionId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Both_MealId_And_InjectionId_Are_Provided()
    {
        var request = new NewTreatmentRequest
        {
            MealId = Guid.NewGuid(),
            InjectionId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }
}