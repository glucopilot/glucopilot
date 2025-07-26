using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;
using NUnit.Framework;
using System;
using static GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment.UpdateTreatmentRequest;

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
    public void Validate_Should_Return_Error_When_All_Update_Values_Are_Invalid()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            Meals = [],
            Ingredients = [],
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
            x => x.ErrorMessage == "INJECTION_ID_REQUIRED_WHEN_ALL_NULL"));
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
                x => x.ErrorMessage == "MEAL_REQUIRED_WHEN_ALL_NULL"));
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
                x => x.ErrorMessage == "READING_ID_REQUIRED_WHEN_ALL_NULL"));
            Assert.That(result.Errors, Has.Exactly(1).Matches<FluentValidation.Results.ValidationFailure>(
                x => x.ErrorMessage == "INGREDIENT_REQUIRED_WHEN_ALL_NULL"));
        });
    }

    [Test]
    public void Validate_Should_Pass_When_Only_InjectionId_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = Guid.NewGuid(),
            Meals = [],
            Ingredients = [],
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Only_Meal_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            Meals = [new UpdateTreatmentMealRequest { Id = Guid.NewGuid(), Quantity = 1 }],
            Ingredients = [],
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Only_Ingredient_Is_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = null,
            Meals = [],
            Ingredients = [new UpdateTreatmentIngredientRequest { Id = Guid.NewGuid(), Quantity = 1 }],
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
            Meals = [],
            Ingredients = [],
            ReadingId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_Should_Pass_When_Multiple_Values_Are_Provided()
    {
        var request = new UpdateTreatmentRequest
        {
            InjectionId = Guid.NewGuid(),
            Meals = [new UpdateTreatmentMealRequest { Id = Guid.NewGuid(), Quantity = 1 }],
            Ingredients = [new UpdateTreatmentIngredientRequest { Id = Guid.NewGuid(), Quantity = 1 }],
            ReadingId = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }
}