using NUnit.Framework;
using Moq;
using GlucoPilot.Api.Endpoints.Meals.UpdateMeal;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Models;
using GlucoPilot.AspNetCore.Exceptions;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;

[TestFixture]
public class UpdateMealTests
{
    private Mock<IValidator<UpdateMealRequest>> _validatorMock;
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;
    private Mock<ICurrentUser> _currentUserMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateMealRequest>>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Test]
    public async Task HandleAsync_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = "",
            MealIngredients = new List<NewMealIngredientRequest>()
        };

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required.")
        });

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Update_Is_Successful()
    {
        var userId = Guid.NewGuid();
        var mealId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var request = new UpdateMealRequest
        {
            Id = mealId,
            Name = "Updated Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    Id = Guid.NewGuid(),
                    IngredientId = ingredientId,
                    MealId = mealId,
                    Quantity = 1
                }
            }
        };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Original Meal",
            MealIngredients = new List<MealIngredient>(),
            Created = DateTimeOffset.UtcNow,
        };

        _mealRepositoryMock
            .Setup(m => m.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(meal);

        _ingredientRepositoryMock
            .Setup(i => i.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
        _mealRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<Meal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Meal_Is_Not_Found()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateMealRequest
        {
            Id = Guid.NewGuid(),
            Name = "Updated Meal",
            MealIngredients = new List<NewMealIngredientRequest>()
        };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        _mealRepositoryMock
            .Setup(m => m.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meal)null);

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("MEAL_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Ingredient_Is_Not_Found()
    {
        var userId = Guid.NewGuid();
        var mealId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var request = new UpdateMealRequest
        {
            Id = mealId,
            Name = "Updated Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    IngredientId = ingredientId,
                    Quantity = 1,
                    Id = Guid.NewGuid(),
                    MealId = mealId,
                }
            }
        };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Original Meal",
            MealIngredients = new List<MealIngredient>(),
            Created = DateTimeOffset.UtcNow,
        };

        _mealRepositoryMock
            .Setup(m => m.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(meal);

        _ingredientRepositoryMock
            .Setup(i => i.AnyAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INGREDIENT_NOT_FOUND"));
    }
}