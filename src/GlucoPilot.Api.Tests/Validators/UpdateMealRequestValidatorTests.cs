using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Meals.UpdateMeal;
using GlucoPilot.Api.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;

[TestFixture]
public class UpdateMealRequestValidatorTests
{
    private UpdateMealRequest.UpdateMealRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateMealRequest.UpdateMealRequestValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            MealIngredients = new List<NewMealIngredientRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Not_Have_Error_When_Name_Is_Provided()
    {
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = "Valid Meal Name",
            MealIngredients = new List<NewMealIngredientRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_When_MealIngredients_Has_Invalid_Ingredient()
    {
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = "Valid Meal Name",
            MealIngredients = new List<NewMealIngredientRequest>
        {
            new NewMealIngredientRequest
            {
                Id = Guid.NewGuid(),
                MealId = Guid.NewGuid(),
                IngredientId = Guid.Empty,
                Quantity = 1
            }
        }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("MealIngredients[0].IngredientId");
    }

    [Test]
    public void Should_Not_Have_Error_When_MealIngredients_Are_Valid()
    {
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = "Valid Meal Name",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    Id = Guid.NewGuid(),
                    MealId = Guid.NewGuid(),
                    IngredientId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.MealIngredients);
    }
}
