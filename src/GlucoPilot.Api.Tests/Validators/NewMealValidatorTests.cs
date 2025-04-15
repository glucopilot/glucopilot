using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Meals.NewMeal;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

[TestFixture]
public class NewMealValidatorTests
{
    private readonly NewMealRequest.NewMealValidator _validator;

    public NewMealValidatorTests()
    {
        _validator = new NewMealRequest.NewMealValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new NewMealRequest { Name = string.Empty, MealIngredients = new List<NewMealIngredientRequest> { } };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_For_Invalid_NewMealIngredientRequest()
    {
        var ingredientValidator = new NewMealIngredientRequest.NewMealIngredientValidator();
        var model = new NewMealIngredientRequest
        {
            Id = Guid.Empty,
            IngredientId = Guid.Empty,
            Quantity = 0
        };

        var result = ingredientValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.IngredientId);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        var model = new NewMealRequest
        {
            Name = "Test Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    Id = Guid.NewGuid(),
                    IngredientId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.MealIngredients);
    }
}