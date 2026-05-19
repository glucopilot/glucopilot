using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Meals.UpdateMeal;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

[TestFixture]
public class UpdateMealTests
{
    private Mock<IValidator<UpdateMealRequest>> _validatorMock;
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;
    private Mock<IRepository<MealIngredient>> _mealIngredientRepositoryMock;
    private Mock<ICurrentUser> _currentUserMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateMealRequest>>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
        _mealIngredientRepositoryMock = new Mock<IRepository<MealIngredient>>();
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

        var validationResult = new ValidationResult([
            new ValidationFailure("Name", "Name is required.")
        ]);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _mealIngredientRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Update_Is_Successful()
    {
        var userId = Guid.NewGuid();
        var mealId = Guid.NewGuid();
        var existingMealIngredientId = Guid.NewGuid();
        var updatedIngredientId = Guid.NewGuid();
        var newIngredientId = Guid.NewGuid();

        var request = new UpdateMealRequest
        {
            Id = mealId,
            Name = "Updated Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    Id = existingMealIngredientId,
                    IngredientId = updatedIngredientId,
                    Quantity = 3
                },
                new NewMealIngredientRequest
                {
                    IngredientId = newIngredientId,
                    Quantity = 1
                }
            }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Original Meal",
            MealIngredients = new List<MealIngredient>
            {
                new MealIngredient
                {
                    Id = existingMealIngredientId,
                    MealId = mealId,
                    IngredientId = Guid.NewGuid(),
                    Quantity = 1
                },
                new MealIngredient
                {
                    Id = Guid.NewGuid(),
                    MealId = mealId,
                    IngredientId = Guid.NewGuid(),
                    Quantity = 2
                }
            },
            Created = DateTimeOffset.UtcNow,
        };

        _mealRepositoryMock
            .Setup(m => m.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new[] { meal }.AsQueryable());

        _ingredientRepositoryMock
            .Setup(i => i.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _mealIngredientRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
        _mealRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<Meal>(), It.IsAny<CancellationToken>()), Times.Once);
        _mealIngredientRepositoryMock.Verify(m => m.DeleteManyAsync(It.IsAny<Expression<Func<MealIngredient, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(meal.Name, Is.EqualTo("Updated Meal"));
        Assert.That(meal.MealIngredients, Has.Count.EqualTo(2));
        Assert.That(meal.MealIngredients.Single(mi => mi.Id == existingMealIngredientId).Quantity, Is.EqualTo(3));
        Assert.That(meal.MealIngredients.Single(mi => mi.Id == existingMealIngredientId).IngredientId, Is.EqualTo(updatedIngredientId));
        Assert.That(meal.MealIngredients.Count(mi => !request.MealIngredients.Where(r => r.Id.HasValue).Select(r => r.Id!.Value).Contains(mi.Id)), Is.EqualTo(1));
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
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mealRepositoryMock
            .Setup(m => m.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(Array.Empty<Meal>().AsQueryable());

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object, _mealIngredientRepositoryMock.Object, CancellationToken.None),
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
                }
            }
        };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Original Meal",
            MealIngredients = new List<MealIngredient>(),
            Created = DateTimeOffset.UtcNow,
        };

        _mealRepositoryMock
            .Setup(m => m.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new[] { meal }.AsQueryable());

        _ingredientRepositoryMock
            .Setup(i => i.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object, _mealIngredientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INGREDIENT_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_MealIngredient_Id_Does_Not_Belong_To_Meal()
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
                    Quantity = 1
                }
            }
        };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Original Meal",
            MealIngredients = new List<MealIngredient>(),
            Created = DateTimeOffset.UtcNow,
        };

        _mealRepositoryMock
            .Setup(m => m.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new[] { meal }.AsQueryable());

        _ingredientRepositoryMock
            .Setup(i => i.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Assert.That(
            async () => await Endpoint.HandleAsync(
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _mealIngredientRepositoryMock.Object,
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("MEAL_INGREDIENT_NOT_FOUND"));
    }
}
