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
    public void Validator_Should_Have_Error_When_Both_Meals_Ingredients_Is_Empty_And_InjectionId_Is_Null()
    {
        var request = new NewTreatmentRequest
        {
            Meals = [],
            Ingredients = [],
            Injection = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.ShouldHaveValidationErrorFor(r => r), Is.Not.Null);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Meal_Is_Provided()
    {
        var request = new NewTreatmentRequest
        {
            Meals = new[]
            {
                new NewTreatmentMeal
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1
                }
            },
            Injection = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Ingredient_Is_Provided()
    {
        var request = new NewTreatmentRequest
        {
            Ingredients = new[]
            {
                new NewTreatmentIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1
                }
            },
            Injection = null
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_InjectionId_Is_Provided()
    {
        var request = new NewTreatmentRequest
        {
            Meals = [],
            Ingredients = [],
            Injection = new NewInjection
            {
                InsulinId = Guid.NewGuid(),
                Units = 1
            }
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_Should_Not_Have_Error_When_Meals_Ingredients_And_InjectionId_Are_Provided()
    {
        var request = new NewTreatmentRequest
        {
            Meals = new[]
            {
                new NewTreatmentMeal
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1
                }
            },
            Ingredients = new[]
            {
                new NewTreatmentIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1
                }
            },
            Injection = new NewInjection
            {
                InsulinId = Guid.NewGuid(),
                Units = 1
            }
        };

        var result = _validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }
}